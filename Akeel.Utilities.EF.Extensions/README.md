# Akeel.Utilities.EF.Extensions

Provider-neutral query criteria, includes, ordering and pagination helpers for Entity Framework Core. Version 3.1.0 targets .NET 10 and declares one dependency: `Microsoft.EntityFrameworkCore [10.0.12,11.0.0)`. Applications on older frameworks need an explicit migration.

```text
dotnet add package Akeel.Utilities.EF.Extensions --version 3.1.0
```

## Ordering and request state

This self-contained example orders an in-memory query and creates independent criteria state. The same ordering extension accepts an EF `IQueryable<T>`; check that your provider can translate your column expressions.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using Akeel.Utilities.EF.Extensions.ExtensionMethods;
using Akeel.Utilities.EF.Extensions.Interfaces;
using Akeel.Utilities.EF.Extensions.Models;

var items = new[]
{
    new Item { Id = 1, Amount = 5 },
    new Item { Id = 3, Amount = 10 },
    new Item { Id = 2, Amount = 10 }
};

var criteria = ListCriteriaDto<string, ItemOrder, ItemInclude>.CreateDefault();
criteria.PaginationInfo = new RequestPaginationInfo { PageIndex = 1, PageSize = 2 };

var ordered = items.AsQueryable().DoOrder(
    new List<ItemOrder> { ItemOrder.AmountDesc, ItemOrder.IdAsc },
    new ItemOrderHelper());

Console.WriteLine(string.Join(",", ordered.Select(item => item.Id))); // 2,3,1
Console.WriteLine(criteria.PaginationInfo.PageSize); // 2

enum ItemOrder { AmountDesc, IdAsc }
enum ItemInclude { Category }

sealed class Item
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
}

sealed class ItemOrderHelper : IOrderableRange<Item, ItemOrder>
{
    public OrderColumn<Item> GetOrderColumn(ItemOrder order) => order switch
    {
        ItemOrder.AmountDesc => OrderColumn<Item, ItemOrder>.CreateOrder(order, item => item.Amount),
        ItemOrder.IdAsc => OrderColumn<Item, ItemOrder>.CreateOrder(order, item => item.Id),
        _ => throw new ArgumentOutOfRangeException(nameof(order))
    };
}
```

`DoOrder` applies the first mapped column as the primary order and later columns as tie-breakers. Empty/null option lists pass through; null mappings are ignored. Supply a unique tie-break column when stable paging matters.

## Query APIs and limits

`DoSearchCriteria` delegates filtering to an `ISearchableCriteria` implementation. `IncludeRange` uses an `IIncludableRange` implementation. `Pagination` applies `Skip`/`Take` for positive page index and size. `OnSelectListWithPaginationAsync` counts the filtered rows with `LongCountAsync`, materializes the page, and returns `RowsWithPaginationInfo<TDto>`. Its overload with a final `CancellationToken` passes cancellation to both EF operations; the older overload remains available. The async helper runs count and materialization sequentially on the same query/context.

Create a new criteria object per request with `ListCriteriaDto<TFilter,TOrder,TInclude>.CreateDefault()`. Its `PaginationInfo`, `ListOrderOptions` and `ListRelatedObjects` initially are null. Null or nonpositive page index/size is treated as unbounded by the query extensions. An empty result has zero pages; a nonempty unbounded result has one. `StaticValueDto.PageSize` does not set pagination. The legacy `ListCriteriaDto.Default` field is mutable and shared; `GetCriteriaDto<TInclude>.Default` is a fresh-instance property.

For bounded pagination, the computed offset must fit `Int32.MaxValue`; a larger offset throws `ArgumentOutOfRangeException` with parameter name `pagination`, including for an empty query. `OrderColumn.CreateOrder` infers ascending direction from an enum name ending in `Asc`; other names mean descending. These helpers do not supply search logic, EF provider configuration or a unique ordering key.

The package was qualified with EF Core 10.0.12 and PostgreSQL/Npgsql 10.0.3; this does not establish behavior for every allowed EF patch or provider. The frontend criteria package has distinct DTO types and is not a dependency of this version. The public 3.1.0 archive contains the earlier embedded README; this source guide reflects the current repository documentation.

Source: [this directory](../Akeel.Utilities.EF.Extensions). License: [MIT](../LICENSE); the published 3.1.0 archive also carries an MIT expression and license file. See [all published versions and source provenance](../docs/package-versions.md) and the [3.1.0 verification record](../docs/development/RELEASE_3_1_0_VERIFICATION.md).
