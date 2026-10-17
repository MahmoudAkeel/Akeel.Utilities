using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;

namespace Akeel.Utilities.EF.Extensions.Tests
{
    internal sealed class OwnedPostgreSqlDatabase : IAsyncDisposable
    {
        private readonly string _name = "akeel_order_" + Guid.NewGuid().ToString("N");
        private readonly string _marker = "akeel-p01:" + Guid.NewGuid().ToString("N");
        private readonly string _adminConnection;
        private readonly string _connection;
        private bool _created;

        private OwnedPostgreSqlDatabase(NpgsqlConnectionStringBuilder settings)
        {
            _adminConnection = settings.ConnectionString;
            settings.Database = _name;
            _connection = settings.ConnectionString;
        }

        public static async Task<OwnedPostgreSqlDatabase> CreateAsync()
        {
            var scope = Environment.GetEnvironmentVariable("AKEEL_POSTGRES_TEST_SCOPE");
            if (scope is null || !Regex.IsMatch(scope, "\\Adisposable-[a-z0-9-]{1,32}\\z") ||
                !int.TryParse(Environment.GetEnvironmentVariable("AKEEL_POSTGRES_TEST_PORT"), out var port) || port is < 1024 or > 65535)
            {
                throw new InvalidOperationException("Set AKEEL_POSTGRES_TEST_SCOPE and AKEEL_POSTGRES_TEST_PORT for the dedicated disposable Compose service. See P01_VERIFICATION.md.");
            }
            await VerifyServiceAsync(scope, port);
            var root = new DirectoryInfo(AppContext.BaseDirectory);
            while (root is not null && !File.Exists(Path.Combine(root.FullName, "Akeel.Utilities.sln"))) root = root.Parent;
            var workspace = root?.FullName ?? throw new InvalidOperationException("Package workspace not found.");
            var secretPath = Path.Combine(workspace, ".local", "postgresql", scope, "password");
            for (var path = secretPath; path is not null; path = Path.GetDirectoryName(path))
            {
                if ((File.Exists(path) || Directory.Exists(path)) && (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidOperationException("Qualification secret paths must not contain links.");
            }
            if (!File.Exists(secretPath) || new FileInfo(secretPath).Length is < 1 or > 1024)
                throw new InvalidOperationException("Create the protected ignored password file described in P01_VERIFICATION.md.");
            var database = new OwnedPostgreSqlDatabase(new NpgsqlConnectionStringBuilder
            {
                Host = "127.0.0.1", Port = port, Database = "postgres", Username = "postgres",
                Password = await File.ReadAllTextAsync(secretPath), Pooling = false, Timeout = 10,
                CommandTimeout = 15, IncludeErrorDetail = false, SslMode = SslMode.Disable
            });
            try
            {
                await using var connection = new NpgsqlConnection(database._adminConnection);
                await connection.OpenAsync();
                await using var create = new NpgsqlCommand($"CREATE DATABASE \"{database._name}\"", connection);
                await create.ExecuteNonQueryAsync();
                await using var comment = new NpgsqlCommand($"COMMENT ON DATABASE \"{database._name}\" IS '{database._marker}'", connection);
                await comment.ExecuteNonQueryAsync();
                database._created = true;
                await using var context = database.Context();
                await context.Database.EnsureCreatedAsync();
                context.Rows.AddRange(OrderingCases.Rows());
                await context.SaveChangesAsync();
                return database;
            }
            catch
            {
                await database.DisposeAsync();
                throw;
            }
        }

        public OrderingDbContext Context(params IInterceptor[] interceptors) => new(new DbContextOptionsBuilder<OrderingDbContext>()
            .UseNpgsql(_connection).AddInterceptors(interceptors).EnableSensitiveDataLogging(false).EnableDetailedErrors(false).Options);

        public async ValueTask DisposeAsync()
        {
            if (!_created) return;
            if (!Regex.IsMatch(_name, "\\Aakeel_order_[a-f0-9]{32}\\z"))
                throw new InvalidOperationException("Unexpected database name; cleanup refused.");
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            await using var connection = new NpgsqlConnection(_adminConnection);
            await connection.OpenAsync(timeout.Token);
            await using var marker = new NpgsqlCommand("SELECT shobj_description(oid, 'pg_database') FROM pg_database WHERE datname = @name", connection);
            marker.Parameters.AddWithValue("name", _name);
            if ((string?)(await marker.ExecuteScalarAsync(timeout.Token)) != _marker)
                throw new InvalidOperationException("Database ownership marker changed; cleanup refused.");
            await using var drop = new NpgsqlCommand($"DROP DATABASE \"{_name}\"", connection);
            await drop.ExecuteNonQueryAsync(timeout.Token);
            _created = false;
        }

        private static async Task VerifyServiceAsync(string scope, int port)
        {
            var project = "akeel-utilities-" + scope;
            using var document = JsonDocument.Parse(await DockerAsync("inspect", "--type", "container", project + "-postgres-1"));
            var container = document.RootElement[0];
            var labels = container.GetProperty("Config").GetProperty("Labels");
            VerifyLabels(labels, project, scope);
            if (labels.GetProperty("com.docker.compose.service").GetString() != "postgres" ||
                !container.GetProperty("State").GetProperty("Running").GetBoolean() ||
                container.GetProperty("State").GetProperty("Health").GetProperty("Status").GetString() != "healthy")
                throw new InvalidOperationException("Wait for the dedicated PostgreSQL service to become healthy.");
            var bindings = container.GetProperty("NetworkSettings").GetProperty("Ports").GetProperty("5432/tcp");
            if (bindings.GetArrayLength() != 1 || bindings[0].GetProperty("HostIp").GetString() != "127.0.0.1" ||
                bindings[0].GetProperty("HostPort").GetString() != port.ToString(System.Globalization.CultureInfo.InvariantCulture))
                throw new InvalidOperationException("Dedicated PostgreSQL port binding differs from the selected scope.");
            var volumes = container.GetProperty("Mounts").EnumerateArray().Where(mount => mount.GetProperty("Type").GetString() == "volume").ToArray();
            var volume = project + "_postgres-data";
            if (volumes.Length != 1 || volumes[0].GetProperty("Name").GetString() != volume ||
                volumes[0].GetProperty("Destination").GetString() != "/var/lib/postgresql")
                throw new InvalidOperationException("Unexpected PostgreSQL volume; qualification refused.");
            using var volumeDocument = JsonDocument.Parse(await DockerAsync("inspect", "--type", "volume", volume));
            VerifyLabels(volumeDocument.RootElement[0].GetProperty("Labels"), project, scope);
        }

        private static void VerifyLabels(JsonElement labels, string project, string scope)
        {
            if (labels.GetProperty("com.docker.compose.project").GetString() != project ||
                labels.GetProperty("akeel.utilities.scope").GetString() != scope ||
                labels.GetProperty("akeel.utilities.repository").GetString() != "MahmoudAkeel/Akeel.Utilities")
                throw new InvalidOperationException("Docker resource does not belong to the selected package test scope.");
        }

        private static async Task<string> DockerAsync(params string[] arguments)
        {
            var start = new ProcessStartInfo("docker")
            {
                UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true
            };
            foreach (var argument in arguments) start.ArgumentList.Add(argument);
            using var process = Process.Start(start) ?? throw new InvalidOperationException("Docker inspection could not start.");
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var output = process.StandardOutput.ReadToEndAsync(timeout.Token);
            var error = process.StandardError.ReadToEndAsync(timeout.Token);
            try { await process.WaitForExitAsync(timeout.Token); }
            catch (OperationCanceledException)
            {
                if (!process.HasExited) process.Kill(true);
                throw new InvalidOperationException("Docker ownership inspection timed out.");
            }
            await error;
            if (process.ExitCode != 0)
                throw new InvalidOperationException("Start the dedicated disposable PostgreSQL service before these tests. See P01_VERIFICATION.md.");
            return await output;
        }
    }
}
