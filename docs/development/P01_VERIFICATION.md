# P01 — EF 10 ordering verification

Recorded: 17 October 2026. Workflow: wf-astra-high-sol6.1-high. Astra high independently reviewed P01 on 17 October 2026 and reported: "No actionable findings remain for P01." P01 awaits user acceptance. No completed-task user acceptance, enhancement commit/push, NuGet publication or application adoption is claimed.

## Baseline and exact scope

Private source repository: [MahmoudAkeel/Akeel.Utilities](https://github.com/MahmoudAkeel/Akeel.Utilities). Original import on main: f0e7ed38199a6de8ac3b404bbafe00e7cace9a41. Local enhancement branch: codex/ef10-ordering-foundation. Only the EF project was retargeted to net10.0; the original solution and other seven projects remain unchanged. The application/Multitenant repositories and their ordering adapter/regressions were not modified.

Production changes: EF Core dependency range [10.0.12,11.0.0), local version 3.0.0-preview.1, explicit pack/metadata/README and the ordering chain fix. Existing package ID, public namespaces/method signatures, object-selector API, null/empty passthrough and skipped mappings remain. Public source-call compatibility is retained; the .NET 10 target is a deliberate major framework change, not binary compatibility with older framework consumers.

The focused test project has no application project reference or private application-state dependency. Its PostgreSQL provider and driver are test-only. The [full upgrade plan](../planning/EF_EXTENSIONS_UPGRADE_PLAN.md) leaves P02 cancellation, P03 pagination/counts, P04 defaults and P05 release/adoption unstarted.

## Original-source failure reproduced before the fix

The imported ordering file was copied unchanged into ignored local evidence before any ordering edit. SHA-256:

~~~text
0C3C0FBCF521F1DCC78E1B47E3D22FB1A066A1837A177909125D9F42239FC5EC
~~~

After selecting the modern framework/dependency graph, the original source was run through four two-column direction combinations in LINQ and the same four on owned PostgreSQL. **All 8 failed, 0 passed, 0 skipped.** Ascending secondary clauses reused the original unordered expression: LINQ observed IndexOutOfRangeException during rewriting/enumeration, while EF 10 observed ArgumentException (arg0) constructing ThenBy with an IQueryable expression where IOrderedQueryable was required. Descending secondary clauses restarted with OrderByDescending, returning ID-only order and losing amount precedence.

This is imported-source-on-modern-graph evidence. It does not claim native EF 5 execution or binary identity with the historical NuGet 2.2.4 package. The final tests assert the correct behavior; no permanently failing or broad expected-exception test remains.

The fix collects the same mapped columns, starts one IOrderedQueryable using OrderBy/OrderByDescending, then carries it through ThenBy/ThenByDescending. Null mappings are skipped. There is no automatic ID sort or new invalid-direction policy; caller-supplied unique tie-breaks are explicit in tests.

## Actual results

| Check | Result |
| --- | --- |
| Focused initial and locked restore | Passed with normal NuGet audit. |
| Selected EF/test Release build | Passed, 0 errors. Recompilation reports the pre-existing CS0108 AddNewInclude hiding warning in unchanged Models/ListCriteria.cs; it was not suppressed. |
| Final complete P01 suite | **35 passed, 0 failed, 0 skipped**: 17 LINQ, 17 real PostgreSQL, 1 existing public-call compatibility case. |
| Missing PostgreSQL environment guard | Explicit failure with setup instruction; no silent skip/pass or database access. |
| Explicit local pack | Passed; net10-only 3.0.0-preview.1 nupkg inspected. |
| Isolated local-package consumer | Passed after fresh-cache restore and locked restore, with exact Akeel source mapping and no ProjectReference. |
| Credential/redaction and cleanup | Passed; owned databases removed and all pre-existing Docker resources preserved. |

The matrix covers all four two-column and eight three-column direction combinations using duplicate values and an explicit unique final ID. Both LINQ and real PostgreSQL cover null, empty, all-skipped, interleaved-skipped and single-order lists. Passthrough cases assert original query identity without claiming an unspecified database row order.

Representative old public calls compile/exercise both DoSearchCriteria overloads, DoOrder, IncludeRange, Pagination, model/default members and AddNewInclude. The old async DTO method signature is compiled through a delegate, not invoked as proof of provider translation; runtime async DTO projection/count behavior belongs to P02. ListCriteriaDto.Default and StaticValueDto.PageSize remain fields, while GetCriteriaDto.Default remains its existing fresh-instance property.

Resolved production EF Core/Abstractions/Analyzers and Microsoft.Extensions dependencies: 10.0.12. Tests additionally resolve EF Relational 10.0.12, Npgsql EF provider 10.0.3 and Npgsql driver 10.0.3. Production has only Microsoft.EntityFrameworkCore as its direct package dependency; no Npgsql, SQL Server/SqlClient or ValidateResult dependency was added. No SQL Server/other-provider compatibility or future EF patch execution is claimed.

## Local package inspection and consumer proof

Artifact: artifacts/packages/Akeel.Utilities.EF.Extensions.3.0.0-preview.1.nupkg (ignored, local only).

~~~text
Package SHA-256:
9F90FAA8CA37762DFD6370CBF317F436579B20B2E623011BCF7C42BE6EA5CE38
DLL SHA-256:
701854D95BCE66363ABC8F98ACED758EC4F8DF5AEB43D71B785605FE463A63CB
~~~

The archive contains the net10 DLL, README and standard package metadata only. Its sole dependency is Microsoft.EntityFrameworkCore [10.0.12, 11.0.0); repository/project URL is the actual private GitHub repository. No stale net5 asset, source/test/private file, credential, certificate, PDB/SourceLink or invented license was included. SDK-generated repository commit metadata points to the original import base; this preview was built from uncommitted enhancements and cannot be reproduced by checking out that baseline commit alone. It is not a release from a fully committed reviewed revision and must not be published.

The ignored .local/package-probe console has only an exact PackageReference to 3.0.0-preview.1. Its NuGet.Config clears inherited sources, maps the exact Akeel package ID to artifacts/packages and maps other dependencies to nuget.org. A fresh .local/package-probe/packages cache prevents global-cache substitution. Both initial/locked restores passed; project.assets.json identifies Akeel as a package. Runtime LINQ checks proved primary/secondary ascending/descending ordering with no adapter. SHA-256 matched the production DLL, archive DLL, fresh-cache DLL and consumer output DLL.

Actual probe commands, after creating that standalone project/configuration:

~~~text
dotnet restore .local/package-probe/Probe.csproj --configfile .local/package-probe/NuGet.Config --packages .local/package-probe/packages
dotnet restore .local/package-probe/Probe.csproj --configfile .local/package-probe/NuGet.Config --packages .local/package-probe/packages --locked-mode
dotnet run --project .local/package-probe/Probe.csproj --configuration Release --no-restore
~~~

This is local package qualification, not publication or application adoption. See [NuGet source mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping) for the exact-ID source precedence used here.

## Reproduce without operational PowerShell scripts

Use .NET SDK 10.0.401 and Docker Desktop with Linux containers. This session selected a portable matching SDK explicitly; no global SDK/settings changed. Build only the selected projects rather than the original multi-project solution:

~~~text
dotnet restore tests/Akeel.Utilities.EF.Extensions.Tests/Akeel.Utilities.EF.Extensions.Tests.csproj --locked-mode
dotnet build tests/Akeel.Utilities.EF.Extensions.Tests/Akeel.Utilities.EF.Extensions.Tests.csproj --configuration Release --no-restore
dotnet test tests/Akeel.Utilities.EF.Extensions.Tests/Akeel.Utilities.EF.Extensions.Tests.csproj --configuration Release --no-build --no-restore --filter "FullyQualifiedName!~OrderingPostgreSqlTests"
~~~

For PostgreSQL, choose a fresh disposable scope and unused loopback port. Confirm its derived Compose project/resources are absent before creation. In your editor create ignored .local/postgresql/compose.env with nonsecret values, for example:

~~~text
AKEEL_POSTGRES_TEST_SCOPE=disposable-p01-example
AKEEL_POSTGRES_TEST_PORT=25436
~~~

Create .local/postgresql/disposable-p01-example/password with a unique generated development password and owner-only permissions, without printing it or putting it in command arguments/source. The recorded run generated 32 random bytes as hex and verified that only the developer account had an allowed ACL rule. Compose mounts this file as a secret; the fixture reads it into memory.

~~~text
docker compose --env-file .local/postgresql/compose.env -f tests/infrastructure/compose.postgresql.yaml up --detach --wait --wait-timeout 240 postgres
dotnet test tests/Akeel.Utilities.EF.Extensions.Tests/Akeel.Utilities.EF.Extensions.Tests.csproj --configuration Release --no-build --no-restore --environment AKEEL_POSTGRES_TEST_SCOPE=disposable-p01-example --environment AKEEL_POSTGRES_TEST_PORT=25436
dotnet pack Akeel.Utilities.EF.Extensions/Akeel.Utilities.EF.Extensions.csproj --configuration Release --no-build --no-restore --output artifacts/packages
~~~

The fixture requires a disposable scope/valid port, exact repository/project/scope/service labels, a healthy container, the expected loopback binding and an owned volume. It rejects linked credential paths. Pooling is disabled and diagnostic secret/error-detail logging is off. Every case creates its own GUID database and run-specific ownership comment; name plus exact comment are checked before database removal. It never drops an existing database by prefix alone or starts/stops Docker services itself.

After tests, inspect the selected project's resources and confirm exactly its expected PostgreSQL container, volume and network with matching labels. Remove only that newly created scope; do not use prune or substitute a primary/existing project:

~~~text
docker compose --env-file .local/postgresql/compose.env -f tests/infrastructure/compose.postgresql.yaml ps --all
docker compose --env-file .local/postgresql/compose.env -f tests/infrastructure/compose.postgresql.yaml down --volumes
~~~

## Recorded ownership, cleanup and limits

The actual scope was disposable-p01-akeel19a on 127.0.0.1:25436, with project akeel-utilities-disposable-p01-akeel19a. Scope state/resources were initially absent; the port was unused. PostgreSQL 18.6 ran from the dedicated pinned Compose image. No application/native/primary database, Keycloak or other service was started or reset.

After tests, PostgreSQL reported zero remaining akeel_order_ databases. Exact scope/repository labels and resource names were verified before cleanup. Only its new container, volume and network were removed. Before/after identity inventories matched all **25 existing containers, 26 volumes and 11 networks**. The protected ignored password file remains; raw local evidence was scanned in memory against its value with no matches and no credential output.

Raw evidence is ignored under .local/p01-evidence/: original source copy/failures, final test output/TRX, packed nuspec, local consumer output and Docker inventories. The local preview and package-probe cache remain ignored. A review snapshot records actual changed-file hashes without requiring a commit.

No cancellation/overflow/default changes, other-project retargeting, application edits, repository publication or NuGet push was performed. EF 5/native legacy runtime and unrelated utility projects were not built or claimed passing. The .NET 10 framework break and the existing CS0108 warning are explicit limitations. Independent review is clear; P01 still awaits user acceptance. P02–P05 remain unstarted and require the user's next approval.
