# Akeel.Utilities

Original source was imported into this private repository at f0e7ed38199a6de8ac3b404bbafe00e7cace9a41. Only Akeel.Utilities.EF.Extensions is being modernized; the other utility projects are preserved.

Version 3.0.0 is published on NuGet.org after clear independent Astra review. Its source was delivered at 804b5e1eb8cd3afabe4e34e2a9b91a5a2f5eda80. Accepted P02 cancellation was delivered at 07560c2014f2e956f6c0a83a616bb8926d93d45b; reviewed P03 pagination was delivered at 9ab540e692b6c7ebf430be966b5e226c20181328.

The user authorized completing P04 and stable 3.1.0, committing/pushing reviewed package source before publishing NuGet. P04 preserves legacy null/unbounded criteria state and mutable fields, adds CreateDefault for fresh request state, and explicitly acknowledges existing method hiding. The 3.1.0 candidate is prepared locally on codex/ef-extensions-p04-stable-3-1, pending independent review, source delivery and final publication qualification. It is not yet published. Application/FEAT-04 adoption remains separate.

- [Upgrade plan, finding dispositions and status](docs/planning/EF_EXTENSIONS_UPGRADE_PLAN.md)
- [EF package usage and 3.1.0 release notes](Akeel.Utilities.EF.Extensions/README.md)
- [Historical P01 verification](docs/development/P01_VERIFICATION.md)
- [Published 3.0.0 qualification](docs/development/RELEASE_3_0_0_VERIFICATION.md)
- [P02 cancellation verification](docs/development/P02_VERIFICATION.md)
- [P03 pagination verification](docs/development/P03_VERIFICATION.md)
- [P04 compatibility verification](docs/development/P04_VERIFICATION.md)
- [3.1.0 candidate qualification and release status](docs/development/RELEASE_3_1_0_VERIFICATION.md)

## License scope

The user approved MIT for the Akeel.Utilities.EF.Extensions project/package only; see its [LICENSE](Akeel.Utilities.EF.Extensions/LICENSE). No repository-wide license is granted for the other projects, and historical package files/metadata were not altered. The GitHub repository remains private.
