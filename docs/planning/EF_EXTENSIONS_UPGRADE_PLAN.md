# EF extensions upgrade plan

Updated: 18 October 2026. Workflow: wf-astra-high-sol6.1-high. P01 is published as 3.0.0 and P02 was accepted after clear Astra high review and delivered at 07560c2014f2e956f6c0a83a616bb8926d93d45b. **P03 pagination implementation and independent review are complete; the user authorized its source delivery as part of completing the package**, on local branch codex/ef-extensions-p03-pagination. The user authorized completing P04, committing/pushing the package sources and publishing the stable update. P04 will preserve legacy behavior and add a fresh-instance factory; MIT is approved for the EF package only. Application work remains separate. Historical release/test receipts remain unchanged.

## Baseline, provenance and goals

Repository: [MahmoudAkeel/Akeel.Utilities](https://github.com/MahmoudAkeel/Akeel.Utilities), verified **private**. Original source import commit: f0e7ed38199a6de8ac3b404bbafe00e7cace9a41 on main. P00 preserved original source plus exclusions for build/user/credential artifacts. Current local branch: codex/ef-extensions-p03-pagination, based on 07560c2014f2e956f6c0a83a616bb8926d93d45b. Original source, reviewed 3.0.0 and accepted P02 were delivered before P03. P03 source delivery is now authorized on codex/ef-extensions-p03-pagination before the P04 branch is created.

The imported EF project declares net5.0, EF Core 5.0.1 and package version 2.2.4. That version string does **not** prove equivalence to the NuGet 2.2.4 binary previously consumed by the application. Preserve the source import and distinguish imported-source failures, historical package observations and newly built preview evidence.

Goals: make ordering correct and directly usable on EF 10; retain established public types/calls; introduce cancellation and robust pagination through separately approved tasks; document compatibility, real-provider evidence and a usable local-package workflow. [EF Core 10 requires .NET 10](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/whatsnew). P01 used a 3.0.0-preview.1 local qualification artifact. The user has now selected stable 3.0.0 for publication; the major version makes the framework break explicit. Production remains provider-neutral, depending only on EF Core with range [10.0.12,11.0.0); do not silently claim EF 11 support.

Non-goals: retarget the other seven projects, rebuild/publish historical 2.2.4, add SQL Server/Npgsql/ValidateResult production dependencies, introduce a repository framework, change application result contracts, copy app tenant contracts or adopt the preview into FEAT-04. Source Multitenant and the application repository remain untouched. No license grant, SourceLink/source publication or release approval is invented.

## Imported source findings

| Source | Finding and implication | Task |
| --- | --- | --- |
| ExtensionMethods/QuerableOrderExtension.cs | Each secondary clause starts from the original query; descending secondary uses OrderByDescending. The chain can throw for ThenBy or discard primary ordering. | P01 |
| Models/OrderColumn.cs | Public Expression<Func<TEntity, object>> selector and direction inference from enum name are existing caller contracts. Keep these APIs and qualify actual EF translation rather than replacing them. | P01 |
| ExtensionMethods/QueryableExtensions.cs | Existing async count/materialization do not accept a CancellationToken; CountAsync and ToListAsync were sequential. | P02 |
| Same file | Page offset multiplies int values without overflow checks; CountAsync returns int even though response totals are long. | P03 |
| Same file | Null or any nonpositive page/index means unbounded rows; empty result has 0 pages, nonempty unbounded result has 1. These are existing semantics to preserve. | P02/P03 |
| Models/ListCriteria.cs | Setter creates a 25-row default and then overwrites it with the original value. A silent fix would change effective null/unbounded behavior to 25 rows. | P04 decision |
| Same file and Models/StaticValueDto.cs | ListCriteriaDto.Default and StaticValueDto.PageSize are public mutable static fields; field-to-property changes break compiled consumers. GetCriteriaDto.Default is already a property returning a new instance. | P04 decision |
| Interfaces, all model files, query/include extension files | Review covers existing public helpers, both DoSearchCriteria overloads, IncludeRange, async DTO list method, model getters/setters, AddNewInclude overloads, static members and enum constraints. P01 changes no public signatures. | P01/P05 |
| Async DTO projection | ToDto currently appears inside the final EF projection. P05 qualified the existing bounded/empty/unbounded DTO path against packed 3.0.0 on PostgreSQL. Cancellation behavior remains a separate P02 task. | P02 |
| EF project file | Automatic packaging on every build and minimal metadata obscure explicit release intent. Use explicit pack, a useful README and the actual private repository URL. | P01 |

Before P01 ordering changes, the unchanged imported ordering source was tested after selecting the modern target graph. All eight focused two-column cases failed across LINQ and real PostgreSQL. Original ordering SHA-256: 0C3C0FBCF521F1DCC78E1B47E3D22FB1A066A1837A177909125D9F42239FC5EC. See [actual verification](../development/P01_VERIFICATION.md); this is source-on-modern-graph evidence, not native EF 5 execution or NuGet 2.2.4 provenance.

## Task sequence and review gates

| Task | Scope | Dependency | Current status |
| --- | --- | --- | --- |
| P00 | Preserve/import original source into private GitHub | User publication authorization | Done; original import only |
| P01 | EF 10 ordering foundation, focused tests, explicit local package and documentation | P00; user acceptance recorded | Accepted by user after clear Astra high review, 17 October 2026 |
| P02 | Add cancellation without removing existing async signature | Accepted P01 and explicit next-task approval | Accepted after clear Astra high review; delivered at 07560c2, 18 October 2026 |
| P03 | Overflow-safe pagination and long counts preserving unbounded semantics | Accepted P02 and explicit approval | Reviewed clear and approved for source delivery under the package-completion instruction, 18 October 2026 |
| P04 | Decide default setter/static-field compatibility, then implement only chosen behavior | Accepted prior tasks and explicit decision/approval | Not started |
| P05 | Stable 3.0.0 qualification/publication of accepted P01; application adoption separate | Accepted P01 and explicit NuGet publication authorization | Published accepted P01 as 3.0.0; stable review clear and fresh public restore/runtime verified; adoption unstarted |

For each task: record the approved boundary → Sol implementation and focused checks → Astra review of a stable diff/evidence → fixes/re-review → present the concrete result → stop for the user's next decision. Approval of implementation does not authorize source commit/push or application adoption. The explicit NuGet authorization applied to stable 3.0.0, now published; the P01/P02 previews were not published and the current P03 preview is not authorized for publication.

### P01 — Modern EF ordering foundation

Purpose: fix evidenced ordering failures in the package itself while keeping legacy utility projects and public calls intact.

Deliverables: EF project net10.0/EF 10.0.12 qualification, EF dependency range [10.0.12,11.0.0), local version 3.0.0-preview.1, explicit packaging/README/repository metadata, focused test project and repository-owned disposable PostgreSQL setup. Keep package ID/namespaces/method signatures, null/empty passthrough, skipped null mappings and the object selector API. Carry an IOrderedQueryable from first OrderBy/OrderByDescending through all ThenBy/ThenByDescending clauses. Do not add an automatic ID tie-break or a new invalid-enum policy.

Acceptance:
- [x] Reproduce original source defects before applying the ordering fix; distinguish expression construction from executed SQL/results.
- [x] Pass all two/three-column direction combinations with duplicate values and explicit unique final keys in LINQ and real PostgreSQL.
- [x] Preserve null/empty/all-skipped/interleaved-skipped/single-order behavior; compile representative existing public calls.
- [x] Locked restore/build selected projects, inspect provider-neutral production graph, explicitly pack and inspect net10 assets/nuspec/README.
- [x] Restore a standalone consumer from the new local package with restricted source mapping and a fresh package cache; prove it uses the package instead of a ProjectReference.
- [x] Verify owned-resource cleanup, safe diagnostics and stable uncommitted diff.
- [x] Astra high independently reviewed P01 on 17 October 2026: "No actionable findings remain for P01."
- [x] User accepted P01 and authorized the P05 stable-release qualification/publication follow-up. At that acceptance, P02–P04 and application adoption remained unstarted; P02 is now separately authorized as recorded above.

Excluded: cancellation, offset/count algorithm changes, default/static-field changes, other utility projects, application edits/adoption and publication.

### P02 — Add cancellable asynchronous materialization

Purpose: allow callers to stop both database operations while preserving source/binary entry points.

Retain the existing async method's three-argument public signature. Add a CancellationToken overload as a distinct required fourth argument; have the original signature forward CancellationToken.None. Propagate that token to count and materialization, sequentially on the same IQueryable/context. Do not run concurrent EF operations or create a context inside the helper.

Acceptance: old/new call sites compile; pre-cancelled calls and cancellation during real active count/materialization stop predictably; existing empty/nonempty/bounded/unbounded DTO results remain compatible. Exercise existing ToDto projection on EF 10, record actual behavior and propose only a necessary, separately reviewed fix if it fails. Keep existing error behavior and do not conceal provider failures.

Evidence: [P02 verification](../development/P02_VERIFICATION.md) records 45 passing project tests, exact backend/blocker observations for active cancellation, the unchanged compiled 3.0.0 caller running with the new DLL, and an isolated actual-package consumer. LongCount/offset changes remain P03.

- [x] Retain the original signature and add a required final token overload forwarding to both sequential EF operations.
- [x] Verify legacy/None/live-token results, pre-cancellation and real active count/materialization cancellation with cleanup and context recovery.
- [x] Preserve compiled old caller and stable artifacts; qualify the isolated local 3.1.0-preview.1 package.
- [x] Astra high independently reviewed P02 on 18 October 2026: "No actionable findings remain for P02."
- [x] User accepted P02 and authorized its source commit/push and P03 implementation. No additional NuGet publication or P03 commit/push is authorized.

### P03 — Prevent offset/count overflow

Purpose: avoid wrapped offsets and incorrect large totals while preserving the current pagination meaning.

Compute a positive bounded page's offset using checked wider arithmetic before calling Skip. Reject larger offsets with ArgumentOutOfRangeException(nameof(pagination)), retaining the actual long offset; no clamp. Compose the page before any SQL, including an empty query. The async helper faults its Task for this invalid input before observing cancellation; valid input retains P02 cancellation. Use LongCountAsync with the P02 token. Compute total pages using overflow-safe long quotient/remainder arithmetic. Preserve null/nonpositive pagination as unbounded, 0 pages for empty data and 1 page for nonempty unbounded data.

Acceptance: first/middle/partial-last/beyond-last/empty pages, null/zero/negative unbounded cases, int offset boundary/overflow and long page-total arithmetic. Prove filtered rows are counted before paging and count/materialization remain sequential. Do not add a hidden default size or silently clamp oversized input.

Evidence: [P03 verification](../development/P03_VERIFICATION.md) records real-provider pagination/count/cancellation, long arithmetic boundaries, unchanged compiled callers and an isolated local-package consumer. Synthetic private-helper reflection tests cover counts beyond int.MaxValue/near long.MaxValue; they do not claim a billion-row database.

- [x] Validate wider offsets before SQL and preserve null/nonpositive unbounded behavior.
- [x] Use LongCountAsync and overflow-safe long page totals without changing public DTO/API signatures.
- [x] Pass all prior and focused tests, frozen-caller checks and actual preview-package consumer checks; preserve owned resources.
- [x] Astra high independently reviewed P03 on 18 October 2026: "No actionable findings remain for P03."
- [x] User authorized P03 source delivery and completing P04/stable publication under the latest package-completion instruction.

### P04 — Decide defaults and shared mutable fields

Purpose: improve usability without accidentally changing observable source or binary contracts.

Before implementation, present choices for the overwritten pagination setter and shared mutable fields. Effective null pagination currently stays null/unbounded. Decide whether to preserve that behavior with clearer code, or intentionally introduce bounded defaults with documented caller migration. Separately decide whether to retain fields and add a fresh-instance factory, or accept a field-to-property binary break. Assess StaticValueDto.PageSize separately; do not pretend it controls ListCriteriaDto's current private size field.

Acceptance follows the explicit decision: independent default instances where selected, documented null/zero behavior, old compiled/source consumers where compatibility is promised and migration notes for any deliberate break. No change is authorized by this plan alone. Follow [library breaking-change guidance](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/breaking-changes).

### P05 — Qualify and publish accepted P01; adoption remains separate

Purpose: qualify and publish accepted P01 as stable 3.0.0 under the user's explicit authorization. P02–P04 are optional future increments, not prerequisites silently added to this release.

Keep production behavior unchanged and prepare 3.0.0 metadata, install/release notes and explicit net10/EF range requirements. Re-run the existing 35 cases and a bounded actual-package PostgreSQL smoke for both search overloads, real includes, bounded paging and the existing async DTO helper (nonempty bounded, empty and unbounded). Use exact local package source mapping and a fresh cache, inspect assets/dependency range/private-file exclusions, and report any observed helper failure before changing behavior or publication. Omit stale baseline commit/revision metadata from the uncommitted working-tree build; retain the actual private URL. Preserve an immutable private source/artifact copy and SHA-256 manifest instead of inventing a source-commit gate.

A later application task must explicitly choose the package version/feed, update its lock graph and re-run affected real-provider contracts before removing its local ordering adapter. The current application's historical bug-expectation regression must then be deliberately revised. Do not edit it as part of P01. Authentication/tenancy and FEAT-04.T02 remain outside this package work.

NuGet 3.0.0 publication is complete and verified. Reviewed 3.0.0 source delivery is complete at commit 804b5e1eb8cd3afabe4e34e2a9b91a5a2f5eda80 on the pushed codex/ef10-ordering-foundation branch; later task commits/pushes and application adoption still require separate authorization. Never overwrite historical 2.2.4 or claim a package was published before actual registry confirmation. See [stable release verification](../development/RELEASE_3_0_0_VERIFICATION.md) for actual results and artifact/source manifests.

## Scoped command entry points

With .NET SDK 10.0.401 selected, run only the EF/test projects; the legacy solution contains other framework targets and is not the P01 build boundary.

~~~text
dotnet restore tests/Akeel.Utilities.EF.Extensions.Tests/Akeel.Utilities.EF.Extensions.Tests.csproj --locked-mode
dotnet build tests/Akeel.Utilities.EF.Extensions.Tests/Akeel.Utilities.EF.Extensions.Tests.csproj --configuration Release --no-restore
dotnet test tests/Akeel.Utilities.EF.Extensions.Tests/Akeel.Utilities.EF.Extensions.Tests.csproj --configuration Release --no-build --no-restore --filter "FullyQualifiedName~OrderingLinqTests|FullyQualifiedName~PublicApiCompatibilityTests"
dotnet pack Akeel.Utilities.EF.Extensions/Akeel.Utilities.EF.Extensions.csproj --configuration Release --no-build --no-restore --output artifacts/p03
~~~

Full provider-test Compose/credential instructions and actual local-package probe results belong to [P01 verification](../development/P01_VERIFICATION.md). No operational PowerShell script or app-private state dependency is introduced.

P02 full-provider and package/binary checks are recorded in [P02 verification](../development/P02_VERIFICATION.md). Its preview output is separate from the preserved stable 3.0.0 artifacts.

P03 current evidence and separate preview output are recorded in [P03 verification](../development/P03_VERIFICATION.md). P02 delivery is complete; its historical verification receipts remain unchanged.
