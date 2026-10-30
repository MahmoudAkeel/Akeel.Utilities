# Maintaining the package documentation

The six libraries have independent package IDs, versions, target frameworks and release histories. Update each library's README and the [version catalog](package-versions.md) when its published behavior changes. Keep README examples tied to an exact published version and run them against that package before describing them as verified.

The repository [MIT license](../LICENSE) covers the source. Keep package licensing and release metadata explicit for each future package: a package license expression or included license, its README, the actual `RepositoryUrl`, and a `RepositoryCommit` matching the delivered source. Confirm the packed nuspec and assets before publication. Older NuGet archives retain their original metadata; changing repository documents or gallery-hosted Markdown does not rewrite them.

Version each package independently. For a verified release source, use a package-scoped tag such as `Akeel.Utilities.EF.Extensions/v3.1.0` and link the source commit, release notes and exact NuGet version. A project file's version number, an imported source tree, or a tag created later does not prove correspondence to a historical published binary. Record an exact public archive comparison when claiming source provenance. Do not assign guessed tags to old releases.
