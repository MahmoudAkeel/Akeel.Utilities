# Public catalog documentation verification

Recorded 30 October 2026 for the documentation and repository-license update. This is a bounded check of the six owned package IDs, 22 version records, current README examples and public repository readiness. It does not establish source equivalence for legacy archives or change any NuGet package.

## Package inventory

The package owner's NuGet version selectors and public NuGet registration metadata agree on 22 published versions across six IDs. The [version catalog](../package-versions.md) records every exact version, target framework, declared dependency range and listing status. Four versions are unlisted: EF.Extensions 2.1.0 and 2.2.0, Frontend.CriteriaDesign 1.0.0, and QueryStringBuilder.Net 1.0.0. They remain available by exact ID/version. EF.Extensions 3.1.0 is the only one of these metadata records with an MIT license expression; the new root MIT source license does not rewrite older archive metadata.

## Initial exact public package examples (launch commit `4c0b689`)

Six disposable console consumers under ignored `.local/docs-examples` extracted the first C# block from each package README as published in commit `4c0b689`. Each project referenced only its exact public package version, with NuGet.org as its only configured source. Restore, compile and execution passed for:

| Package | Version | Observed output |
| --- | --- | --- |
| EF.Extensions | 3.1.0 | `2,3,1` and `2` |
| ExtensionMethods | 1.0.1 | `42` and `True` |
| Frontend.CriteriaDesign | 1.0.2 | `Category` and `20` |
| ValidateResult | 1.0.3 | `True: Enter a positive integer` |
| QueryStringBuilder.Net | 1.0.0 | `?name=red+pen&page=2` |
| FluentQueryStringBuilder.Net | 1.0.0 | `?name=red+pen&page=2` |

The exact public QueryStringBuilder.Net archive revealed a mismatch with the imported repository source. Its public `Add` accepts `QueryStringRecord[]`; the imported source has `Add(string, string)`. The public sample was corrected and rerun. A separate probe observed that public `Add` retains the first case-insensitive duplicate, accepts an empty key/value record and returns `?` for an empty builder. This supports the explicit provenance caveat in its README and the catalog. Compiling one sample per package does not verify every historical version or every API path.

## Full EF request-to-result sample

The later [EfProductSearch sample](../../samples/EfProductSearch) expands the EF guide into a complete query flow. It restores exact public `Akeel.Utilities.EF.Extensions [3.1.0]` and sample-only `Microsoft.EntityFrameworkCore.Sqlite [10.0.12]` from NuGet.org; its lock file records the resolved dependencies. A locked restore and execution passed on .NET 10. With five seeded products and a fresh change tracker, the active/minimum-price filter leaves three rows, price/ID ordering selects Blue pen then Red pen, category inclusion populates both DTOs, and page 1 of size 2 reports three total rows and two pages.

An ignored `.local/ef-product-search-probe` consumer reuses the sample model and helper. It asserts the two ordered IDs, loaded category names, three total rows and two pages, then checks three edges against the same public package and SQLite provider: an empty filtered result returns zero rows/pages; omitting `Category` leaves its DTO field null; and a pre-cancelled valid request throws `OperationCanceledException`. These checks do not qualify every provider or all EF package APIs.

## Repository publication boundary

Before the documentation update, the remote had seven commits across five refs and no tags. A bounded filename/content pattern scan of tracked history found no high-confidence credential match; it is not a comprehensive secret guarantee. Historical development records contain nonsecret internal paths and project references and remain labeled historical. The hosted repository had no Actions runs, open or closed issues, open or closed pull requests, or GitHub releases/assets at the time of audit. GitHub tags and releases for the verified EF 3.0.0 and 3.1.0 source commits are planned as separate publication steps; no historical tags are inferred from project versions.

The new root license covers repository source for all six libraries. The existing EF package license file and historical release verification records are preserved. Gallery-hosted README edits, if made, are separate from immutable published nupkg README/nuspec contents.
