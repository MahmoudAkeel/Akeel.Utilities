# Akeel.Utilities.Frontend.CriteriaDesign

Request DTOs for filters, related-object names, order-option names and pagination. Version 1.0.2 targets .NET 5 and declares no package dependencies. These types live in `Akeel.Utilities.Frontend.CriteriaDesign`; the current EF extensions package has its own DTO types in a different namespace.

```text
dotnet add package Akeel.Utilities.Frontend.CriteriaDesign --version 1.0.2
```

```csharp
using System;
using System.Collections.Generic;
using Akeel.Utilities.Frontend.CriteriaDesign;

var criteria = new ListCriteriaDto<string, ProductOrder, ProductInclude>
{
    Entity = "pencil",
    ListOrderOptions = new List<string> { nameof(ProductOrder.NameAsc) },
    PaginationInfo = new RequestPaginationInfo { PageIndex = 1, PageSize = 20 }
};
criteria.AddNewInclude(ProductInclude.Category);

Console.WriteLine(criteria.ListRelatedObjects[0]); // Category
Console.WriteLine(criteria.PaginationInfo.PageSize); // 20

enum ProductOrder { NameAsc }
enum ProductInclude { Category }
```

`GetCriteriaDto<TInclude>` offers `AddNewInclude` and a fresh-instance `Default` property. `ListCriteriaDto<TFilter,TOrder,TInclude>` adds `Entity`, `ListOrderOptions` and `PaginationInfo`. The package also has request/response pagination DTOs, `RowsWithPaginationInfo<T>` and `OrderColumn`.

The `ListCriteriaDto.Default` field is shared and mutable; create a new DTO per request. Its `PaginationInfo` setter's attempted fallback is overwritten by the assigned value, so assign valid pagination explicitly. `StaticValueDto.PageSize` does not initialize request pagination. The package targets .NET 5, which is no longer supported by Microsoft; check your application's framework before using it.

Source: [this directory](../Akeel.Utilities.Frontend.CriteriaDesign). License: [MIT](../LICENSE). See [published versions and source provenance](../docs/package-versions.md).
