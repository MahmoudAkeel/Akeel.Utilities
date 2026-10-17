# Akeel.Utilities

Original utility source was imported into this private repository at commit f0e7ed38199a6de8ac3b404bbafe00e7cace9a41. Existing projects are preserved; only Akeel.Utilities.EF.Extensions is being upgraded.

The user accepted P01 after clear independent Astra review and authorized stable NuGet publication. Version 3.0.0 is published on NuGet.org after clear Astra high artifact review. A fresh public-feed-only restore and runtime ordering/DLL identity check passed. The reviewed 3.0.0 source was committed as 804b5e1eb8cd3afabe4e34e2a9b91a5a2f5eda80 and pushed on codex/ef10-ordering-foundation. The published package was built from the reviewed working tree; its private immutable source/artifact copies and hash manifests remain the publication evidence. Later task commits/pushes and application adoption require their own authorization.

- [Detailed upgrade plan and current task status](docs/planning/EF_EXTENSIONS_UPGRADE_PLAN.md)
- [EF package usage and 3.0.0 release notes](Akeel.Utilities.EF.Extensions/README.md)
- [Historical P01 preview evidence](docs/development/P01_VERIFICATION.md)
- [Stable release qualification](docs/development/RELEASE_3_0_0_VERIFICATION.md)
- [P02 cancellation verification](docs/development/P02_VERIFICATION.md)

P02 cancellation is implemented locally on codex/ef-extensions-p02-cancellation as unpublished 3.1.0-preview.1, independently reviewed by Astra high on 18 October 2026 with no actionable findings; user acceptance is pending. The user authorized its source commit/push and the start of P03; NuGet publication of the preview remains unauthorized. Overflow/default enhancements and application adoption remain unstarted.
