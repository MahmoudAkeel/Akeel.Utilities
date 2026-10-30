# Akeel.Utilities

Small .NET utility libraries published as six independent NuGet packages. Pick the package that matches your use case; each has its own version and target framework. EF extensions is the actively maintained library here; the other five are documented as legacy packages without a promised update schedule. The [package version catalog](docs/package-versions.md) records every published version and what historical source can be verified.

| Package | Use it for | Current published version | Target framework | Usage |
| --- | --- | --- | --- | --- |
| [Akeel.Utilities.EF.Extensions](https://www.nuget.org/packages/Akeel.Utilities.EF.Extensions/3.1.0) | Entity Framework Core criteria, ordering, includes and pagination | 3.1.0 | .NET 10; EF Core 10 | [README](Akeel.Utilities.EF.Extensions/README.md) |
| [Akeel.Utilities.Frontend.CriteriaDesign](https://www.nuget.org/packages/Akeel.Utilities.Frontend.CriteriaDesign/1.0.2) | Request criteria and pagination DTOs | 1.0.2 | .NET 5 | [README](Akeel.Utilities.Frontend.CriteriaDesign/README.md) |
| [Akeel.Utilities.ValidateResult](https://www.nuget.org/packages/Akeel.Utilities.ValidateResult/1.0.3) | Success or failure values with an error code and message | 1.0.3 | .NET Standard 2.0 | [README](Akeel.Utilities.ValidateResult/README.md) |
| [Akeel.Utilities.ExtensionMethods](https://www.nuget.org/packages/Akeel.Utilities.ExtensionMethods/1.0.1) | Small string, collection, date and other extensions | 1.0.1 | .NET Standard 2.0 | [README](Akeel.Utilities.ExtensionMethods/README.md) |
| [QueryStringBuilder.Net](https://www.nuget.org/packages/QueryStringBuilder.Net/1.0.0) | Build a query string from key/value pairs or nested objects | 1.0.0 | .NET Standard 2.0 | [README](QueryStringBuilder.Net/README.md) |
| [FluentQueryStringBuilder.Net](https://www.nuget.org/packages/FluentQueryStringBuilder.Net/1.0.0) | Build a query string with a fluent API in its own namespace | 1.0.0 | .NET Standard 2.0 | [README](FluentQueryStringBuilder.Net/README.md) |

Install one package with `dotnet add package <package-id> --version <version>`. The package READMEs have exact commands and examples. The query string packages have different public `Add` signatures in their published 1.0.0 archives; check their individual guides. The frontend DTOs and EF extensions also have distinct types; the frontend package is not a dependency of the current EF package.

This repository's source is [MIT licensed](LICENSE). Published NuGet archives and their historical metadata are unchanged by this source license. See the [version catalog](docs/package-versions.md) for the limits of historical source correspondence.

Maintainers: [documentation and release metadata](docs/maintaining-packages.md). Historical EF upgrade and qualification records remain under [docs/development](docs/development) and [docs/planning](docs/planning).
