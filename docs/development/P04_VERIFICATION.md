# P04 criteria compatibility verification

Recorded: 18 October 2026. Workflow: wf-astra-high-sol6.1-high. The user authorized remaining package completion, reviewed source commit/push and stable NuGet publication. P04 implementation is ready for independent review; the parent will deliver source before final publication. No new user-acceptance gate is introduced. Application adoption remains separate.

Baseline: 9ab540e692b6c7ebf430be966b5e226c20181328, delivered P03. Current local branch: codex/ef-extensions-p04-stable-3-1. These changes are unstaged/uncommitted.

## Chosen compatibility behavior

PaginationInfo remains a public getter/setter of RequestPaginationInfo, storing the original reference, including null/zero/negative inputs. Its old setter allocated a 25-row default and then overwrote that allocation with the supplied value. The auto-property removes that ineffective work/private state; it does not introduce a new default or bound unbounded queries.

ListCriteriaDto.Default and StaticValueDto.PageSize retain public mutable static field identity, types and assignment. CreateDefault() is additive and returns a new legacy-state instance with null pagination/order/include lists, without reading/cloning/mutating Default. Callers should use the fresh factory per request and set bounded pagination explicitly. Legacy fields remain mutable for compiled callers; this known shared-state risk is documented rather than silently changed. StaticValueDto.PageSize does not automatically configure criteria.

GetCriteriaDto.Default remains a fresh-instance property. The existing derived instance AddNewInclude now explicitly uses new; its signature, return identity, deduplication and base behavior remain. CS0108 is removed without suppression. Enum parsing and other utility projects are unchanged. The edited class file was normalized to UTF-8 without BOM.

## Actual evidence

Normal audited restore, locked restore and scoped Release build passed on SDK 10.0.401, with zero warnings/errors. Existing lock files remained unchanged.

Full owned-PostgreSQL suite: **77 passed, 0 failed/skipped**, comprising all 68 previous cases plus nine criteria cases:

- Five constructor/setter/JSON cases cover null, zero, negative index, negative size and positive pagination, supplied-reference identity and unchanged order/include serialization. Enum convenience properties remain ignored.
- Fresh factories are independent of each other and of a deliberately replaced/mutated shared Default, including when that legacy field is null; one instance's assigned lists/pagination do not affect others.
- Reflection and real assignments verify declared field types and mutable read/write behavior; original static values are restored in finally. Criteria tests use a dedicated closed generic type.
- Base Default still creates fresh instances. Derived/base include calls retain object identity and deduplicate includes, including the existing static null-input path.

Before any production changes, a small console caller was compiled against exact preserved packed 3.0.0 from an exact local feed. It exercises constructor/null/zero pagination getter/setter reference identity and reads/writes both legacy fields with finally restoration. Original caller SHA-256: E12E4DCE86220603849DEC89010182E6808572579572413045010EC8ACCDFFB9.

Its compiled output was frozen under .local/release31-evidence/field-caller before rebuilding production. Only Akeel.Utilities.EF.Extensions.dll was replaced with the final candidate library, and the unchanged caller passed. Every other frozen output file matches its original manifest. This proves field/accessor binary entry points in addition to newly compiled source tests.

Separate unchanged compiled legacy 3.0.0 async and P02 token callers also passed 3/3 and 2/2 against the candidate DLL. No tracked ReleaseSmoke project/source/lock changed. Original P01–P03 artifacts and receipts were preserved.

[3.1.0 release verification](RELEASE_3_1_0_VERIFICATION.md) records the actual package consumer, MIT/metadata inspection, hash identity and cleanup. The candidate factory and retained fields were also exercised through the actual packed package.

Raw proof: .local/release31-evidence/results/release31.trx, release31-tests.txt, field-binary.txt, field-caller-original-manifest.json, p02/legacy caller manifests and binary output/TRX. The [upgrade plan](../planning/EF_EXTENSIONS_UPGRADE_PLAN.md) records explicit dispositions for all remaining findings.

P04 is ready for independent review. Commit/push and final NuGet release are authorized in the parent sequence after review; neither was performed by this implementation phase.
