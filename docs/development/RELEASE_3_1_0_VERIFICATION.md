# Stable 3.1.0 release qualification

Recorded: 18 October 2026. Workflow: wf-astra-high-sol6.1-high. **Independent Astra source and final-artifact reviews are clear; source commit/push and final qualification are complete. NuGet 3.1.0 is not published yet: manual final-file selection and registry confirmation remain pending.** The user explicitly authorized package completion and publication, with reviewed source delivered before the final NuGet package. Application/FEAT-04 adoption is outside this work.

## Source and release scope

Current branch: codex/ef-extensions-p04-stable-3-1, based on delivered P03 9ab540e692b6c7ebf430be966b5e226c20181328. Reviewed P04/release source was committed/pushed as 813822b72b4a296ab76a3bc299a83eea7dfbabe5 before final packing. The baseline identifies the historical candidate preparation context; candidate receipts below remain unchanged. Historical P01/P02/P03 evidence, preview packages and published 3.0.0 archives remain unchanged.

Version 3.1.0 includes the accepted cancellation overload, safe long-count pagination/offset validation, fresh criteria factory with legacy state/fields preserved, explicit existing include-method hiding, and user-approved MIT for this EF package only. No production provider, application result, repository framework, SourceLink or source/symbol package was added. Public 3.0/P02 entry points remain.

SDK: exact portable 10.0.401 at D:/Interactive/Multi-tenant/AgileStart/ISA.MultiTenant.Platform/.tools/dotnet-sdk-10.0.401/dotnet.exe, with process-local DOTNET_ROOT/PATH for VSTest children; no global setting changed. Target net10.0; production EF [10.0.12,11.0.0), resolved Core/Abstractions 10.0.12. Test-only Npgsql EF/driver 10.0.3. Qualification does not promise all allowed patches/providers.

## Historical candidate qualification

| Check | Result |
| --- | --- |
| Normal audited restore, locked restore, selected Release build | Passed; 0 warnings/errors; locks unchanged |
| Full owned-PostgreSQL suite | 77 passed, 0 failed/skipped |
| Caller compiled against old packed 3.0.0: legacy fields/accessors | Passed without recompilation |
| Frozen compiled P02 token and legacy 3.0.0 async callers | 2/2 and 3/3 passed without recompilation |
| Fresh-cache exact-local-package candidate consumer | 3 passed, 0 failed/skipped; build 0 warnings/errors |
| Nupkg MIT/README/assets/dependency/hash inspection | Passed |
| Exact owned cleanup, all prior resource identities | Passed; 25 containers, 26 volumes, 11 networks preserved |

The 77-case suite includes nine new [P04 compatibility cases](P04_VERIFICATION.md), all previous ordering/pagination/long arithmetic checks and deterministic active cancellation/context recovery. Active count helper/blocker PIDs were 353/356; active materialization after count used 312/314. Those tests observe exact active PostgreSQL lock/blocker identity before cancelling, then drain and roll back in finally and successfully reuse the helper context. Synthetic near-long.MaxValue arithmetic tests do not claim huge-row database execution.

Three ignored .local/release31-package-probe tests reference exact [3.1.0], not the production project. Cleared source mapping pins the exact Akeel ID to artifacts/release31 and other dependencies to NuGet.org. A new private cache/lock was populated with normal audit restore, then locked restore. Tests exercise:

- Assembly informational version exactly 3.1.0 without stale Git suffix; fresh factory/state independence and real legacy field assignments.
- Both real search helpers, duplicate-safe multi-column ordering, actual included navigation loading, direct bounded pagination, legacy async and required-token async helpers with filtered long row/page totals.
- Oversized-offset rejection on an empty query and valid-input pre-cancellation, followed by successful context recovery.

Before production edits, the old field/accessor caller was compiled against packed 3.0.0 and frozen. Separately frozen P02/3.0 async callers were retained. Only each copied runtime library was replaced; all other output hashes still match original manifests. Original legacy caller and P02 proof folders were not edited.

## Candidate artifact, license and identity

Candidate: artifacts/release31/Akeel.Utilities.EF.Extensions.3.1.0.nupkg
SHA-256: EEF35AA8125EFE49C3B52A17EC841415EFE37FE164EB1CA7108F6673E2C8CB79

Library SHA-256: C354B8FBD4E775E77E37547B6C53B6BDDD9A86B743CEFF745E53A6FC09B9A6EB. This is identical in build output, nupkg, isolated cache, consumer runtime and all three frozen caller runtimes. The restored nupkg hash matches the candidate artifact.

The nupkg contains one net10.0 DLL, README.md, LICENSE and standard NuGet metadata. Packaged README/LICENSE match source byte-for-byte. Nuspec has license type expression MIT, stable 3.1.0, the actual private repository URL and sole EF dependency [10.0.12,11.0.0). It contains no candidate-baseline commit claim. No private state, credential, source/PDB or extra production dependency is packaged.

MIT was explicitly approved for Akeel.Utilities.EF.Extensions only. Full [LICENSE](../../Akeel.Utilities.EF.Extensions/LICENSE), copyright (c) 2026 Mahmoud Akeel, follows the [official MIT text](https://opensource.org/license/mit). Its SHA-256 is 673B47872E0341869E5E4332622E62E9762033E9B1A13C24D1EDCCD810F82081. NuGet metadata uses the documented [PackageLicenseExpression](https://learn.microsoft.com/en-us/nuget/reference/msbuild-targets#packagelicenseexpression) plus explicitly packed LICENSE. No root license/other-project grant is added; prior package metadata was not retroactively changed.

This candidate was built from uncommitted reviewed-source preparation; 9ab540e identifies its baseline, not a commit reproducing this candidate. Keep the candidate unchanged as evidence. That source-first sequence has now completed through final artifact qualification, as recorded below; publication has not occurred.

Preserved historical package hashes: 3.0.0 = 13627A74F1901E567665283C93D8C2D3960B2A8E73503BFC8A1534425D7BEF2B; P02 preview = E52781008B16731F7A6BD0EA09BC762C30F994957F010C2A06CE1145ADE4CC09; P03 preview = BAD50C05059628EFBAA8E6B88C147B16C1F1BE19D51F4238C208148B8F2CACA5.

## Reproduction and resource cleanup

Use the exact SDK above from this repository. Fresh scope disposable-release31-factory22a and unused loopback port 25440 were selected after absence/port checks. Its owner-only private password follows the existing [P01 setup](P01_VERIFICATION.md). Set the corresponding AKEEL_POSTGRES_TEST_SCOPE/PORT in the invoking process. Nonsecret Compose environment is .local/release31-evidence/compose.env.

~~~text
docker compose --env-file .local/release31-evidence/compose.env -f tests/infrastructure/compose.postgresql.yaml up --detach --wait --wait-timeout 240 postgres
dotnet restore tests/Akeel.Utilities.EF.Extensions.Tests/Akeel.Utilities.EF.Extensions.Tests.csproj -p:NuGetAudit=true
dotnet restore tests/Akeel.Utilities.EF.Extensions.Tests/Akeel.Utilities.EF.Extensions.Tests.csproj --locked-mode -p:NuGetAudit=true
dotnet build tests/Akeel.Utilities.EF.Extensions.Tests/Akeel.Utilities.EF.Extensions.Tests.csproj -c Release --no-restore
dotnet test tests/Akeel.Utilities.EF.Extensions.Tests/Akeel.Utilities.EF.Extensions.Tests.csproj -c Release --no-build --no-restore --logger "trx;LogFileName=release31.trx" --results-directory .local/release31-evidence/results
dotnet .local/release31-evidence/field-caller/Probe.dll
dotnet vstest .local/release31-evidence/p02-caller/Probe.dll --Logger:"trx;LogFileName=p02-binary.trx" --ResultsDirectory:.local/release31-evidence/results
dotnet vstest .local/release31-evidence/legacy-caller/Akeel.Utilities.EF.Extensions.ReleaseSmoke.Tests.dll --TestCaseFilter:FullyQualifiedName~Packed_existing_async_DTO_helper --Logger:"trx;LogFileName=legacy-binary.trx" --ResultsDirectory:.local/release31-evidence/results
dotnet pack Akeel.Utilities.EF.Extensions/Akeel.Utilities.EF.Extensions.csproj -c Release --no-build --no-restore --output artifacts/release31
dotnet restore .local/release31-package-probe/Probe.csproj --configfile .local/release31-package-probe/NuGet.Config --packages .local/release31-package-probe/packages -p:NuGetAudit=true
dotnet restore .local/release31-package-probe/Probe.csproj --configfile .local/release31-package-probe/NuGet.Config --packages .local/release31-package-probe/packages --locked-mode -p:NuGetAudit=true
dotnet build .local/release31-package-probe/Probe.csproj -c Release --no-restore
dotnet test .local/release31-package-probe/Probe.csproj -c Release --no-build --no-restore --logger "trx;LogFileName=packed-candidate.trx" --results-directory .local/release31-evidence/results
docker compose --env-file .local/release31-evidence/compose.env -f tests/infrastructure/compose.postgresql.yaml down --volumes
~~~

For binary proof, freeze caller outputs/manifests before changing production, then replace only the library DLL. The old-field caller project/config/source and candidate consumer source/config/lock/cache are retained in ignored .local folders. No new operational framework/script was introduced.

Before Compose cleanup, exact project/scope/repository labels were verified on the single created container, volume and network. Zero test databases remained after exact-name/marker per-database cleanup. Password ACL permitted only the developer account; raw evidence and changed source were checked against its in-memory value without printing it. The exact release31 scope was removed; full before/after identities match for all prior 25/26/11 resources. No prune, native/app/primary reset or foreign deletion occurred.

Receipts under .local/release31-evidence include full suite and caller/consumer TRX/output, all original caller manifests, package-inspection.json, artifact-hashes.json, production-packages.txt, docker-before/after.json and review-file-snapshot.json. Final diff/link/XML/JSON/scope checks passed. No functional code changed after these passing checks.

## Final source/artifact delivery and publication status

Independent Astra source review and final-artifact review are clear, 18 October 2026. Release code was committed/pushed on codex/ef-extensions-p04-stable-3-1 as 813822b72b4a296ab76a3bc299a83eea7dfbabe5, with the remote matching and clean source verified before final packing.

Final package: artifacts/release31-final/Akeel.Utilities.EF.Extensions.3.1.0.nupkg
SHA-256: 23C41BD4F228D9DFAED5767E8C5B25194E28A85948B7A0A6917257DF6C49D172

The parent packed --no-build/--no-restore with RepositoryCommit=813822b72b4a296ab76a3bc299a83eea7dfbabe5. Final nuspec contains that exact SHA, stable 3.1.0, MIT expression, the actual private URL and only EF [10.0.12,11.0.0). LICENSE/README match source byte-for-byte and package assets match the preserved candidate. Final DLL is byte-identical to the reviewed candidate, SHA-256 C354B8FBD4E775E77E37547B6C53B6BDDD9A86B743CEFF745E53A6FC09B9A6EB. Both candidate/final archives remain preserved.

A new ignored .local/release31-final-consumer used exact [3.1.0] mapped to the final local feed, with a new cache and lock. Audited and locked restores passed; Release build had zero warnings/errors. Runtime smoke passed version, fresh factory, mutable legacy fields/accessors, serialization, ordering, pagination and oversized-offset rejection. Both async signatures bind. Restored nupkg and runtime DLL hashes match the exact final artifact. Real-provider 77-case/caller/candidate evidence transfers through verified DLL byte identity; PostgreSQL was not recreated for this final metadata-only package check.

Final receipts: .local/release31-evidence/final-package-inspection.json, final-nuspec.xml, final-consumer-smoke.txt and final-consumer-hashes.json. The final consumer source/config/cache/lock remain ignored; historical candidate and earlier records are preserved.

Publication is authorized but NOT performed/confirmed. It is awaiting manual selection of the exact final artifact above in the signed-in NuGet upload flow, then registry confirmation. There is no additional user-acceptance gate. Do not substitute the candidate archive or claim 3.1.0 is published before confirmation. MIT remains EF-package-only, and application adoption remains separate.
