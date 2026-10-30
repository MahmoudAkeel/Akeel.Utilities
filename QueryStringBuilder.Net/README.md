# QueryStringBuilder.Net

Build a URL query string from key/value records or a nested object. Published version 1.0.0 is unlisted on NuGet, but remains installable by exact version. It targets .NET Standard 2.0 and depends on `System.ComponentModel.Annotations` 5.0.0.

```text
dotnet add package QueryStringBuilder.Net --version 1.0.0
```

```csharp
using System;
using QueryStringBuilder.Net;

var query = QueryStringBuilder.Net.QueryStringBuilder.Instance
    .Add(new QueryStringRecord("name", "red pen"),
         new QueryStringRecord("page", "2"))
    .Build();

Console.WriteLine(query); // ?name=red+pen&page=2
```

In the published 1.0.0 archive, `Instance` creates a new builder and `Add(params QueryStringRecord[])` accepts key/value records. `AddComplexObject(obj, removeNullOrEmpty: true)` converts public object properties, including nested objects. The repository's imported source has a different `Add(string, string)` signature, so use the published signature above when consuming 1.0.0.

`Build()` always prefixes `?`, including for an empty builder. In the published package, `Add` keeps the first record for each key without regard to case and accepts empty keys/values. Check `AddComplexObject` output when encoding collections or nested values matters; the imported source has not been shown equivalent to the published archive.

Imported source candidate: [this directory](../QueryStringBuilder.Net). License for repository source: [MIT](../LICENSE). The historical archive's metadata was not changed by this source license. See [published versions and source provenance](../docs/package-versions.md).
