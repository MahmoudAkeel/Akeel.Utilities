# Akeel.Utilities.EF.Extensions

Provider-neutral query criteria, include, ordering and pagination helpers for Entity Framework Core.

**Version 3.0.0 requires .NET 10.** The EF Core dependency range is [10.0.12,11.0.0). Qualification uses EF Core 10.0.12 and PostgreSQL with Npgsql EF/driver 10.0.3; this does not claim every future EF patch or database provider was tested. Applications targeting older frameworks must migrate explicitly before upgrading.

~~~text
dotnet add package Akeel.Utilities.EF.Extensions --version 3.0.0
~~~

## Release notes — 3.0.0

- Correct multi-column ordering in the package: secondary ascending/descending clauses preserve earlier order precedence.
- Preserve public package ID, namespaces, source-call signatures and the existing object-selector column API. The .NET 10 framework change is a major upgrade, not compatibility with older framework binaries.
- Preserve null/empty order passthrough and ignore helper mappings that return null. Callers choose their own unique pagination tie-break columns; no ID order is injected.
- Keep legacy pagination/default behavior. Cancellation overloads and overflow-safe pagination are separate future tasks and are not included in 3.0.0.

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

The package depends only on EF Core. Database providers remain application choices; no repository, UnitOfWork or application result dependency is introduced. DoSearchCriteria, IncludeRange, Pagination and the existing OnSelectListWithPaginationAsync source signatures remain available. Null/nonpositive pagination retains the legacy unbounded meaning; current default fields/setter behavior is unchanged.

Source: [MahmoudAkeel/Akeel.Utilities](https://github.com/MahmoudAkeel/Akeel.Utilities), a private repository requiring access. Source provenance, tested scope and release qualification are documented there. No historical 2.2.4 binary equivalence is claimed.
