# Akeel.Utilities.EF.Extensions

Provider-neutral query criteria, includes, ordering and pagination helpers for Entity Framework Core.

**Version 3.1.0 requires .NET 10.** Production depends only on EF Core [10.0.12,11.0.0). Qualification uses EF Core 10.0.12 and PostgreSQL with Npgsql EF/driver 10.0.3; it does not claim every allowed patch/provider was tested. Applications on older frameworks must migrate explicitly.

~~~text
dotnet add package Akeel.Utilities.EF.Extensions --version 3.1.0
~~~

## Release notes — 3.1.0

- Add a required final CancellationToken overload to OnSelectListWithPaginationAsync. Existing calls remain available and forward CancellationToken.None. Count/materialization run sequentially on the same query/context.
- Count original filtered rows with LongCountAsync and compute long page totals with safe integer division/remainder.
- Validate positive page offsets before SQL: offsets through Int32.MaxValue are supported; larger offsets throw ArgumentOutOfRangeException with ParamName "pagination" and the actual long offset. No clamp/default is injected. Even empty queries reject invalid offsets; the async helper faults its Task before cancellation for invalid input. Valid-input cancellation is preserved.
- Add ListCriteriaDto<TFilter,TOrder,TInclude>.CreateDefault() for independent request state. Preserve null/nonpositive pagination, getter/setter reference identity, mutable legacy fields, JSON shape and existing include behavior.
- Include the approved MIT license for this EF package. No provider, application result, repository or UnitOfWork dependency is added.

The 3.0.0 ordering correction is retained: primary ordering flows through secondary ascending/descending clauses. Null/empty lists pass through and null mappings are ignored. Callers supply their own unique tie-break column.

## Criteria and cancellation

Create a new criteria instance per request with CreateDefault(). It returns the legacy initial state: PaginationInfo, ListOrderOptions and ListRelatedObjects are null. Assign pagination explicitly when bounded results are required:

~~~csharp
var criteria = ListCriteriaDto<ItemFilter, ItemOrder, ItemRelated>.CreateDefault();
criteria.PaginationInfo = new RequestPaginationInfo { PageIndex = 1, PageSize = 25 };

var result = await rows.OnSelectListWithPaginationAsync(
    criteria.PaginationInfo, criteria.ListRelatedObjectsEnum, helper, cancellationToken);
~~~

ItemFilter, ItemOrder, ItemRelated and helper are caller-defined types/implementations. The overload without the final token remains available.

Null or any nonpositive page index/size remains unbounded. Empty results have zero pages; nonempty unbounded results have one page. The legacy Default and StaticValueDto.PageSize remain public mutable fields for compiled consumers; do not share Default between requests. PageSize does not set criteria pagination automatically. GetCriteriaDto<TInclude>.Default remains a property returning a fresh instance.


## Ordering usage

Implement the existing column helper, then pass the supported options to DoOrder:

~~~csharp
using System;
using System.Collections.Generic;
using System.Linq;
using Akeel.Utilities.EF.Extensions.ExtensionMethods;
using Akeel.Utilities.EF.Extensions.Interfaces;
using Akeel.Utilities.EF.Extensions.Models;

public enum ItemOrder { AmountAsc, AmountDesc, IdAsc, IdDesc }

public sealed class Item
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
}

public sealed class ItemOrderHelper : IOrderableRange<Item, ItemOrder>
{
    public OrderColumn<Item> GetOrderColumn(ItemOrder order) => order switch
    {
        ItemOrder.AmountAsc or ItemOrder.AmountDesc =>
            OrderColumn<Item, ItemOrder>.CreateOrder(order, item => item.Amount),
        ItemOrder.IdAsc or ItemOrder.IdDesc =>
            OrderColumn<Item, ItemOrder>.CreateOrder(order, item => item.Id),
        _ => throw new ArgumentOutOfRangeException(nameof(order))
    };
}

public static class ItemQueries
{
    public static IQueryable<Item> OrderForPage(IQueryable<Item> rows) =>
        rows.DoOrder(new List<ItemOrder> { ItemOrder.AmountDesc, ItemOrder.IdAsc },
            new ItemOrderHelper());
}
~~~

The public package ID, namespaces, object-selector ordering API and existing signatures remain. Compatibility is qualified for net10 callers; the 3.0 framework migration was a major change and does not support older target-framework binaries.

## License and source

This EF package is licensed under [MIT](LICENSE), copyright (c) 2026 Mahmoud Akeel. The license does not cover the other utility projects in the repository. Source: [MahmoudAkeel/Akeel.Utilities](https://github.com/MahmoudAkeel/Akeel.Utilities), a private repository requiring access. No equivalence to the historical NuGet 2.2.4 binary is claimed.
