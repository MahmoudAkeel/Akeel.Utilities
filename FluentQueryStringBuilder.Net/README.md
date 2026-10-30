# FluentQueryStringBuilder.Net

Build a URL query string with a fluent API. Version 1.0.0 targets .NET Standard 2.0 and depends on `System.ComponentModel.Annotations` 5.0.0. Its published `Add(string, string)` signature differs from `QueryStringBuilder.Net` 1.0.0, which accepts `QueryStringRecord` values.

```text
dotnet add package FluentQueryStringBuilder.Net --version 1.0.0
```

```csharp
using System;
var query = FluentQueryStringBuilder.Net.QueryStringBuilder.Instance
    .Add("name", "red pen")
    .Add("page", "2")
    .Build();

Console.WriteLine(query); // ?name=red+pen&page=2
```

`Instance` creates a new builder. `Add(key, value)` adds a single pair. `AddComplexObject(obj, removeNullOrEmpty: true)` recursively converts public properties, using dotted keys for nested objects and repeated keys for collections. `[NotMapped]` properties are skipped.

`Build()` always prefixes `?`, including for an empty builder. `Add` ignores empty keys/values and duplicate keys without regard to case. Object collection values are encoded while collected and again during `Build`, so avoid that path when you need standard single encoding. `AddComplexObject` does not apply `Add`'s duplicate suppression.

Source: [this directory](../FluentQueryStringBuilder.Net). License: [MIT](../LICENSE). See [published versions and source provenance](../docs/package-versions.md).
