# P02 cancellation verification

Recorded: 18 October 2026. Workflow: wf-astra-high-sol6.1-high. Astra high independently reviewed P02 on 18 October 2026: "No actionable findings remain for P02." User acceptance is pending. P02 source delivery is now authorized; NuGet publication and application adoption are not. The user also authorized P03 on a separate local branch, with no P03 commit/push and a stop after review.

## Boundary and environment

Branch: codex/ef-extensions-p02-cancellation. Baseline HEAD: 804b5e1eb8cd3afabe4e34e2a9b91a5a2f5eda80, the reviewed 3.0.0 source delivered on origin/codex/ef10-ordering-foundation. Historical P01 and stable-release evidence was preserved.

The original public async signature remains and forwards CancellationToken.None to a new overload with a required final CancellationToken. Only CountAsync and ToListAsync receive that token; operations remain sequential. Ordering, DTO projection, int count/offset arithmetic, null/nonpositive pagination, defaults, dependencies and other utility projects retain their behavior. Source encoding was normalized to UTF-8 without BOM in the edited query file.

Local package version: 3.1.0-preview.1, unpublished. Target: net10.0. Production dependency: Microsoft.EntityFrameworkCore [10.0.12,11.0.0), resolved 10.0.12. Test-only Npgsql EF/driver: 10.0.3. Qualification covers this graph and PostgreSQL, not every allowed patch/provider.

SDK 10.0.401 was selected through the existing portable executable at D:/Interactive/Multi-tenant/AgileStart/ISA.MultiTenant.Platform/.tools/dotnet-sdk-10.0.401/dotnet.exe. Global SDK settings were unchanged. Child VSTest received process-local DOTNET_ROOT/PATH for this SDK.

## Executed checks

| Check | Actual result |
| --- | --- |
| Selected project normal NuGet-audited restore, locked restore and Release build | Passed; existing lock files unchanged |
| Existing and new project tests on real PostgreSQL | 45 passed, 0 failed/skipped: 35 existing + 10 P02 |
| Frozen compiled 3.0.0 caller with only the library DLL replaced | 3 passed, without rebuilding the caller |
| Fresh-cache exact-local-package consumer | 2 passed on PostgreSQL; build 0 warnings/errors |
| Explicit preview pack and contents/dependency/hash inspection | Passed |
| Exact owned resource cleanup and before/after identity comparison | Passed; original 25 containers, 26 volumes, 11 networks preserved |

The production build reports the pre-existing CS0108 warning for ListCriteriaDto.AddNewInclude hiding its base member. It was not changed or suppressed. This is not a zero-warning production-build claim.

### Cancellation and compatibility observations

Seven result-equivalence scenarios compare the legacy call, explicit None and an uncancelled cancellable token: bounded, empty, null, zero/negative index and zero/negative size. They assert rows, row/page totals, exact tokens delivered to EF and count-before-materialization order. Empty results run count only. Nonpositive/null pagination remains unbounded; empty has zero pages and nonempty unbounded has one page.

The pre-cancelled case throws OperationCanceledException and the context remains usable.

Both active cancellation tests use a stable opened helper connection, a separate blocker transaction and separate observer connection within one uniquely owned database. An ACCESS EXCLUSIVE lock blocks the selected operation. Cancellation happens only after pg_stat_activity shows the exact helper PID active with a Lock wait and pg_blocking_pids contains the exact blocker PID; polling/deadlines are bounded. No query SQL is rewritten.

- Active count: helper PID 269, blocker PID 271. Cancellation throws OperationCanceledException; materialization never starts.
- Active materialization: helper PID 238, blocker PID 240. Count completed before the test interceptor acquired the blocking lock immediately before materialization. Cancellation throws OperationCanceledException.
- Both tests cancel and await pending work and roll back the blocker in finally, then successfully query eight rows on the same helper context.

The original 3.0.0 ReleaseSmoke output was copied before production changes to .local/p02-evidence/legacy-caller. Its original per-file manifest is .local/p02-evidence/legacy-caller-original-manifest.json. Only Akeel.Utilities.EF.Extensions.dll was replaced. Every other file still matches that manifest. Caller DLL SHA-256 remained 690D0B58A3A0B988AE984DACD6E965D60122DC1F2B419960B9EE2404E81984EC. The three already-compiled bounded/empty/unbounded async DTO cases passed. The 3.0.0 version-metadata assertion was deliberately excluded; no tracked ReleaseSmoke source/project was changed.

The first frozen-caller invocation omitted the required nonsecret port variable and failed closed before database access (three setup failures). After setting the port, all three passed. Both receipts are preserved; the setup failure is not a package regression. An earlier combined command never executed because automatic permission review timed out; the bounded retry executed successfully.

### Actual-package qualification

Ignored .local/p02-package-probe contains the standalone Probe.csproj, source, NuGet.Config and lock file. It has an exact [3.1.0-preview.1] PackageReference and no ProjectReference. Package sources are cleared: the exact Akeel ID maps to artifacts/p02 and remaining packages to NuGet.org. A fresh .local/p02-package-probe/packages cache was populated with normal audit restore, followed by locked restore.

The packed consumer executes the new required-token overload on PostgreSQL: live-token bounded IDs 3/4 with eight rows/four pages, and a pre-cancelled token throwing OperationCanceledException followed by a successful context query. Deep active-operation cancellation is covered by the project tests above.

Artifact: artifacts/p02/Akeel.Utilities.EF.Extensions.3.1.0-preview.1.nupkg
SHA-256: E52781008B16731F7A6BD0EA09BC762C30F994957F010C2A06CE1145ADE4CC09

Library SHA-256: DCBABBB8F9A09FFBA01EA99A4267BA8ACAE9BB85D088D69E2162F2DA9450A05F, identical in build output, nupkg, isolated cache, consumer output and frozen-caller runtime.

The package contains README.md, its net10.0 DLL and standard NuGet metadata only. Its sole production dependency is the EF range above; there is no provider, SqlClient, ValidateResult, source, symbols, credentials or repository commit metadata. It was built from the uncommitted P02 working tree, not from baseline commit 804b. The baseline commit identifies provenance, not reproducibility of this preview. Stable 3.0.0 package SHA-256 remains 13627A74F1901E567665283C93D8C2D3960B2A8E73503BFC8A1534425D7BEF2B; released artifacts and historical frozen copies were not overwritten.

## Commands and resource lifecycle

Use SDK 10.0.401 for these commands from the package repository. Select a fresh disposable Compose scope/unused loopback port and create its owner-only private password using the existing [P01 setup instructions](P01_VERIFICATION.md); do not reuse another scope. This run used disposable-p02-cancel19a and port 25438. The nonsecret Compose env file was .local/p02-evidence/compose.env.

~~~text
docker compose --env-file .local/p02-evidence/compose.env -f tests/infrastructure/compose.postgresql.yaml up --detach --wait --wait-timeout 240 postgres
dotnet restore tests/Akeel.Utilities.EF.Extensions.Tests/Akeel.Utilities.EF.Extensions.Tests.csproj -p:NuGetAudit=true
dotnet restore tests/Akeel.Utilities.EF.Extensions.Tests/Akeel.Utilities.EF.Extensions.Tests.csproj --locked-mode -p:NuGetAudit=true
dotnet build tests/Akeel.Utilities.EF.Extensions.Tests/Akeel.Utilities.EF.Extensions.Tests.csproj -c Release --no-restore
dotnet test tests/Akeel.Utilities.EF.Extensions.Tests/Akeel.Utilities.EF.Extensions.Tests.csproj -c Release --no-build --no-restore --logger "trx;LogFileName=p02.trx" --results-directory .local/p02-evidence/results
dotnet vstest .local/p02-evidence/legacy-caller/Akeel.Utilities.EF.Extensions.ReleaseSmoke.Tests.dll --TestCaseFilter:FullyQualifiedName~Packed_existing_async_DTO_helper --Logger:"trx;LogFileName=legacy-binary.trx" --ResultsDirectory:.local/p02-evidence/results
dotnet pack Akeel.Utilities.EF.Extensions/Akeel.Utilities.EF.Extensions.csproj -c Release --no-build --no-restore --output artifacts/p02
dotnet restore .local/p02-package-probe/Probe.csproj --configfile .local/p02-package-probe/NuGet.Config --packages .local/p02-package-probe/packages -p:NuGetAudit=true
dotnet restore .local/p02-package-probe/Probe.csproj --configfile .local/p02-package-probe/NuGet.Config --packages .local/p02-package-probe/packages --locked-mode -p:NuGetAudit=true
dotnet build .local/p02-package-probe/Probe.csproj -c Release --no-restore
dotnet test .local/p02-package-probe/Probe.csproj -c Release --no-build --no-restore --logger "trx;LogFileName=packed-preview.trx" --results-directory .local/p02-evidence/results
docker compose --env-file .local/p02-evidence/compose.env -f tests/infrastructure/compose.postgresql.yaml down --volumes
~~~

Set AKEEL_POSTGRES_TEST_SCOPE and AKEEL_POSTGRES_TEST_PORT in the invoking process for tests, including VSTest; this run used the scope and port above. VSTest also needs the exact portable SDK runtime selected in its child environment. Do not rebuild the frozen caller: copy its output before changing production, then replace only the library DLL.

Before cleanup, exact project/scope/repository labels were checked on the single created container, volume and network. Zero akeel_order_* databases remained after per-test exact-name/marker cleanup. The private password ACL allowed only the developer account; evidence was checked in memory against its value without printing it. Compose removed only akeel-utilities-disposable-p02-cancel19a. Before/after full identity inventories match. No app/native/primary resource, Docker prune or reset was used.

Private receipts: .local/p02-evidence/p02-tests.txt, results/p02.trx, legacy-binary-tests.txt, results/legacy-binary.trx, packed-preview-tests.txt, results/packed-preview.trx, artifact-hashes.json, docker-before.json, docker-after.json and review-file-snapshot.json. Ignored probe sources/cache and frozen caller stay local for independent review. Git diff checks and documentation-link checks passed at freeze.

P02 independent Astra high review is clear as of 18 October 2026; user acceptance is pending and the preview is not released. Stop before P03, application changes and any P02 commit/push/publication.


## Acceptance and source delivery follow-up

The user accepted P02 on 18 October 2026 and authorized committing/pushing the reviewed changes on codex/ef-extensions-p02-cancellation. Verification results above describe the reviewed pre-delivery working tree. The new instruction authorizes P03 implementation only, followed by review and user acceptance; it does not authorize P03 commit/push or another NuGet publication.
