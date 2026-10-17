# Stable 3.0.0 release qualification

Recorded: 17 October 2026. The user accepted P01 after clear Astra high review and explicitly authorized stable NuGet publication. Release qualification and independent Astra high artifact review completed with no actionable findings; **3.0.0 is published and its public restore is verified**. No source enhancement commit/push or application adoption was performed. A Git commit is not a prerequisite for this authorized NuGet upload.

## Release contents and provenance

Package: Akeel.Utilities.EF.Extensions **3.0.0**, net10.0, with sole direct dependency Microsoft.EntityFrameworkCore **[10.0.12,11.0.0)**. Ordering behavior/public source-call signatures remain the accepted P01 implementation. Metadata/readme now describe stable installation and the framework break. Cancellation, overflow/count algorithm changes and defaults were not implemented; P02–P04 and application adoption remain unstarted.

The uncommitted working tree is the release source, based on original import f0e7ed38199a6de8ac3b404bbafe00e7cace9a41. That baseline alone does not reproduce the enhancements. Source-control queries and revision suffixes are disabled for this build: nuspec repository commit is absent and actual AssemblyInformationalVersion is exactly **3.0.0**. The real private repository URL remains. No license, SourceLink, source/symbol package or provider dependency is invented.

A private owner-restricted immutable copy of the reviewed working tree, source SHA-256 manifest and exact nupkg is retained under ignored .local/release30-evidence/frozen-release/. The package copy and source files are read-only. This preserves provenance without adding a commit/publication gate. Historical [P01 preview evidence](P01_VERIFICATION.md) and its snapshots remain unchanged.

## Actual validation

| Check | Result |
| --- | --- |
| Selected-project locked restore with normal audit | Passed. |
| Release build of EF project/existing test project | Passed, 0 errors; the unchanged CS0108 AddNewInclude hiding warning remains explicit. |
| Existing ordering/API suite on fresh PostgreSQL scope | **35 passed, 0 failed, 0 skipped.** |
| Explicit stable pack | Passed; net10 DLL, README and standard metadata only. |
| Fresh-cache package-consumer initial/locked restore and build | Passed; exact Akeel package source maps to local artifacts, no ProjectReference. |
| Actual packed-package real PostgreSQL smoke | **4 passed, 0 failed, 0 skipped.** |
| Actual packed assembly metadata check | **1 passed**, informational version exactly 3.0.0. |
| Package/build/fresh-cache/runtime DLL identity | SHA-256 matches at every location. |
| Secret/ownership/database cleanup and before/after inventories | Passed; no remaining test databases or unrelated resource changes. |
| NuGet upload/public registry confirmation | Passed: owner browser submission, public registry version, fresh NuGet.org-only restore and runtime DLL/ordering check. |

The new tracked tests/Akeel.Utilities.EF.Extensions.ReleaseSmoke.Tests project consumes exact PackageReference 3.0.0. It reuses only the test database/model source through linked compile items; it does not reference the production project. The small shared test model gained a note navigation for a real IncludeRange assertion. Existing production helpers were not changed.

The four real-provider smoke cases exercise both DoSearchCriteria overloads against filtered rows, IncludeRange loading actual notes, bounded Pagination, and the existing OnSelectListWithPaginationAsync on nonempty bounded, empty and unbounded queries with expected rows/totals. The async DTO helper executed successfully; no projection/behavior fix was necessary. The informational-version assertion ran separately after adding that metadata-only test. These results qualify the exercised EF 10.0.12/Npgsql 10.0.3 paths, not every future provider/patch or deferred cancellation/overflow behavior.

## Exact artifact

~~~text
artifacts/packages/Akeel.Utilities.EF.Extensions.3.0.0.nupkg
Package SHA-256:
13627A74F1901E567665283C93D8C2D3960B2A8E73503BFC8A1534425D7BEF2B

lib/net10.0/Akeel.Utilities.EF.Extensions.dll
DLL SHA-256:
EF8E06059BB2CC68A4E0CA46C61E96287D0018DC673459408FAEA6E32E1671D8
~~~

Archive inspection confirmed version 3.0.0, the correct EF range and private repository URL, no repository commit/revision claim, and no stale net5/private/source/test/certificate/PDB assets. The same DLL hash was verified in the build output, nupkg, fresh package cache and executing release-smoke output. project.assets.json reports the dependency as a package.

## Reproduction commands

Use SDK 10.0.401. This session selected the matching portable SDK explicitly, without global settings changes. From the repository root:

~~~text
dotnet restore tests/Akeel.Utilities.EF.Extensions.Tests/Akeel.Utilities.EF.Extensions.Tests.csproj --locked-mode
dotnet build tests/Akeel.Utilities.EF.Extensions.Tests/Akeel.Utilities.EF.Extensions.Tests.csproj --configuration Release --no-restore
dotnet pack Akeel.Utilities.EF.Extensions/Akeel.Utilities.EF.Extensions.csproj --configuration Release --no-build --no-restore --output artifacts/packages
~~~

Set up a fresh dedicated Compose scope/owner-only ignored password file as described in the historical P01 setup, replacing its example scope/port. The recorded stable run used disposable-release30-19a on loopback 25437. No application-private setup paths are required. The full existing suite uses the same two nonsecret test environment settings.

After packing, restore the reusable release-smoke project with its exact local source map and a fresh ignored cache:

~~~text
dotnet restore tests/Akeel.Utilities.EF.Extensions.ReleaseSmoke.Tests/Akeel.Utilities.EF.Extensions.ReleaseSmoke.Tests.csproj --configfile tests/Akeel.Utilities.EF.Extensions.ReleaseSmoke.Tests/NuGet.Config --packages .local/release30-packages
dotnet restore tests/Akeel.Utilities.EF.Extensions.ReleaseSmoke.Tests/Akeel.Utilities.EF.Extensions.ReleaseSmoke.Tests.csproj --configfile tests/Akeel.Utilities.EF.Extensions.ReleaseSmoke.Tests/NuGet.Config --packages .local/release30-packages --locked-mode
dotnet build tests/Akeel.Utilities.EF.Extensions.ReleaseSmoke.Tests/Akeel.Utilities.EF.Extensions.ReleaseSmoke.Tests.csproj --configuration Release --no-restore
dotnet test tests/Akeel.Utilities.EF.Extensions.ReleaseSmoke.Tests/Akeel.Utilities.EF.Extensions.ReleaseSmoke.Tests.csproj --configuration Release --no-build --no-restore --environment AKEEL_POSTGRES_TEST_SCOPE=disposable-release30-19a --environment AKEEL_POSTGRES_TEST_PORT=25437
~~~

The recorded four integration cases and one metadata check passed in separate invocations. A fresh-cache source map ensures the Akeel package came from local artifacts, not a previous preview/global-cache package. After exact ownership checks, remove only the newly created Compose scope; do not reset primary/existing projects or use prune.

## Cleanup and evidence locations

The stable scope/resources were absent and port unused before creation. Its protected ignored credential was read only into memory and verified developer-only. PostgreSQL reported zero remaining akeel_order_ databases after all tests. Exact scope/project/repository labels and expected names were checked before removing the new container, volume and network. Before/after identities matched all **25 pre-existing containers, 26 volumes and 11 networks**. No app/native/primary service or data was started/reset.

Ignored .local/release30-evidence/ contains test outputs/TRX, packed nuspec, artifact hashes, Docker inventories and the final review snapshot. The frozen-release/ subdirectory holds the private source/artifact copies and manifests; no credentials or private runtime state are copied. The generated password and fresh test package cache remain ignored locally.

Astra high independently cleared the stable artifact with no actionable findings. NuGet submission, validation and public availability are confirmed. Source is still unstaged/uncommitted; the original private GitHub main remains the imported baseline. No user approval is inferred for source push, future helper changes or application adoption.

## Public publication confirmation

Published package: [Akeel.Utilities.EF.Extensions 3.0.0](https://www.nuget.org/packages/Akeel.Utilities.EF.Extensions/3.0.0), owner MahmoudAkeel. The user selected the reviewed nupkg manually after Chrome blocked automated file selection. The parent verified the NuGet metadata and rendered README, then submitted under the user's explicit authorization. NuGet first confirmed upload/validation pending, then exposed 3.0.0 in its public flat-container index and displayed its install command/README without a pending-validation notice.

The fresh ignored .local/release30-public-verify consumer has only an exact [3.0.0] PackageReference, no ProjectReference, one nuget.org source and fresh package/HTTP caches. Initial restore with --no-cache, locked restore and Release execution all passed. NuGet's cached package metadata records https://api.nuget.org/v3/index.json as source. Runtime informational version is exactly 3.0.0; both ascending/descending chained-order cases passed. The downloaded and executing DLL SHA-256 matches reviewed EF8E06059BB2CC68A4E0CA46C61E96287D0018DC673459408FAEA6E32E1671D8.

NuGet displayed a nonblocking missing-license advisory. No license was invented or added to this release. The source repository remains private and its enhancements remain unstaged/uncommitted/unpushed. P02–P04, application adoption and FEAT-04.T02 remain unstarted.

Local public-consumer evidence: .local/release30-public-verify/restore.log, locked-restore.log, run.log, packages.lock.json, project assets and cache metadata. Publication screenshots: D:/Interactive/Multi-tenant/AgileStart/evidence/akeel-utilities/nuget-3.0.0-submitted.png and nuget-3.0.0-published.png. Historical frozen source/artifact copies and pre-publication review snapshots are preserved; only post-publication documentation status was updated.
