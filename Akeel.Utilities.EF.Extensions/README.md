# Akeel.Utilities.EF.Extensions

Build list endpoints from a request's filter, order, related-object and page choices. This package supplies Entity Framework Core query extensions and criteria/pagination DTOs. Your application defines which filters, sort columns, relationships and response fields are allowed for each entity. Put those rules in a reusable helper, then controllers or services can apply the same request-to-result flow without repeating query wiring.

Version 3.1.0 targets .NET 10 and depends on `Microsoft.EntityFrameworkCore [10.0.12,11.0.0)`. The complete [EfProductSearch sample](../samples/EfProductSearch) uses the exact public 3.1.0 NuGet package and SQLite 10.0.12, with an in-memory database that needs no server.

| Supplied by the package | Supplied by your application |
| --- | --- |
| `ListCriteriaDto`, pagination DTOs and `DoSearchCriteria`, `IncludeRange`, `DoOrder`, `OnSelectListWithPaginationAsync` | EF model/context, request filter and option enums, helper implementations, authorized base query and DTO shape |
| Delegation to your helper and count/page calculation | Actual filter expressions, allowed order mappings, EF `Include` calls and entity-to-DTO mapping |

Only `ProductDbContext` below inherits a base class (`DbContext`). `ProductQueryHelper` implements four package interfaces; there is no package helper base class to inherit.

## Run the complete example

From the repository root:

```text
dotnet run --project samples/EfProductSearch
```

For an existing .NET 10 application, install the library with `dotnet add package Akeel.Utilities.EF.Extensions --version 3.1.0`. The sample's [project file](../samples/EfProductSearch/EfProductSearch.csproj) pins the exact public version and adds SQLite only for this example. It enables implicit .NET usings used by the three C# files below.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Akeel.Utilities.EF.Extensions" Version="[3.1.0]" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="[10.0.12]" />
  </ItemGroup>
</Project>
```

### 1. Define the entity, request choices and result

[ProductModels.cs](../samples/EfProductSearch/ProductModels.cs) defines the types used below. `Program.cs` seeds five products with integer cent prices so SQLite can order them without decimal translation concerns. The filter and enums describe the choices accepted by this example. The DTO is the shape returned to a caller. `ProductDbContext` is ordinary EF Core setup, not a type from this package.

```csharp
using Microsoft.EntityFrameworkCore;

namespace EfProductSearch;

public sealed class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public sealed class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public bool IsActive { get; set; }
    public int PriceCents { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}

public sealed record ProductFilter(bool ActiveOnly, int MinimumPriceCents);
public sealed record ProductDto(int Id, string Name, int PriceCents, string? CategoryName);

public enum ProductOrder { PriceDesc, IdAsc }
public enum ProductInclude { Category }

public sealed class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
}
```

### 2. Implement the four library interfaces

[ProductQueryHelper.cs](../samples/EfProductSearch/ProductQueryHelper.cs) is the entity-specific rule set. `HandleSearchCriteria` applies the permitted filters to an `IQueryable<Product>`. `GetOrderColumn` maps each accepted enum to one sortable expression; `PriceDesc` is primary and unique `IdAsc` breaks ties. `HandleIncludeRange` chooses the EF relationship to load. `ToDto` copies fields into the response and includes the category name only when requested.

```csharp
using Akeel.Utilities.EF.Extensions.Interfaces;
using Akeel.Utilities.EF.Extensions.Models;
using Microsoft.EntityFrameworkCore;

namespace EfProductSearch;

public sealed class ProductQueryHelper :
    ISearchableCriteriaSingleTenant<Product, ProductFilter>,
    IOrderableRange<Product, ProductOrder>,
    IIncludableRange<Product, ProductInclude>,
    ISelectListWithPagination<Product, ProductDto, ProductInclude>
{
    public IQueryable<Product> HandleSearchCriteria(IQueryable<Product> rows, ProductFilter filter)
    {
        if (filter.ActiveOnly)
            rows = rows.Where(product => product.IsActive);

        return rows.Where(product => product.PriceCents >= filter.MinimumPriceCents);
    }

    public OrderColumn<Product> GetOrderColumn(ProductOrder order) => order switch
    {
        ProductOrder.PriceDesc => OrderColumn<Product, ProductOrder>.CreateOrder(order, product => product.PriceCents),
        ProductOrder.IdAsc => OrderColumn<Product, ProductOrder>.CreateOrder(order, product => product.Id),
        _ => throw new ArgumentOutOfRangeException(nameof(order))
    };

    public IQueryable<Product> HandleIncludeRange(IQueryable<Product> rows, ProductInclude include) => include switch
    {
        ProductInclude.Category => rows.Include(product => product.Category),
        _ => throw new ArgumentOutOfRangeException(nameof(include))
    };

    public ProductDto ToDto(Product product, List<ProductInclude> includes)
    {
        var categoryName = includes.Contains(ProductInclude.Category)
            ? product.Category?.Name
            : null;

        return new ProductDto(product.Id, product.Name, product.PriceCents, categoryName);
    }
}
```

`IncludeRange` calls `HandleIncludeRange` to load `Category` from the database. Passing `ProductInclude.Category` to the final pagination helper alone does not load it; that flag tells `ToDto` whether to populate `CategoryName`. Both steps are needed for this response. The helper works with an EF query, so filters and ordering can be translated by the provider before materialization.

### 3. Create a request and run the query

[Program.cs](../samples/EfProductSearch/Program.cs) opens one SQLite in-memory connection, creates and seeds the database, then clears the change tracker. Clearing tracked seed entities prevents navigation fixup from hiding a missing `Include`. `CreateDefault()` gives this request its own `ListCriteriaDto<ProductFilter, ProductOrder, ProductInclude>`. Its `Entity` property holds a `ProductFilter` request, not an EF `Product` entity; `ListOrderOptions` contains `ProductOrder` names as strings; `AddNewInclude` records the `ProductInclude.Category` name; and `PaginationInfo` requests the first two rows.

```csharp
using Akeel.Utilities.EF.Extensions.ExtensionMethods;
using Akeel.Utilities.EF.Extensions.Models;
using EfProductSearch;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using var connection = new SqliteConnection("Data Source=:memory:");
await connection.OpenAsync();

var options = new DbContextOptionsBuilder<ProductDbContext>()
    .UseSqlite(connection)
    .Options;
await using var db = new ProductDbContext(options);
await db.Database.EnsureCreatedAsync();

var pens = new Category { Name = "Pens" };
var paper = new Category { Name = "Paper" };
db.Products.AddRange(
    new Product { Id = 1, Name = "Notebook", IsActive = true, PriceCents = 500, Category = paper },
    new Product { Id = 2, Name = "Blue pen", IsActive = true, PriceCents = 900, Category = pens },
    new Product { Id = 3, Name = "Red pen", IsActive = true, PriceCents = 900, Category = pens },
    new Product { Id = 4, Name = "Pencil", IsActive = true, PriceCents = 200, Category = pens },
    new Product { Id = 5, Name = "Marker", IsActive = false, PriceCents = 1200, Category = pens });
await db.SaveChangesAsync();
db.ChangeTracker.Clear();

var criteria = ListCriteriaDto<ProductFilter, ProductOrder, ProductInclude>.CreateDefault();
criteria.Entity = new ProductFilter(ActiveOnly: true, MinimumPriceCents: 500);
criteria.ListOrderOptions = [nameof(ProductOrder.PriceDesc), nameof(ProductOrder.IdAsc)];
criteria.AddNewInclude(ProductInclude.Category);
criteria.PaginationInfo = new RequestPaginationInfo { PageIndex = 1, PageSize = 2 };

var helper = new ProductQueryHelper();
var rows = db.Products.AsNoTracking()
    .DoSearchCriteria(criteria.Entity, helper)
    .IncludeRange(criteria.ListRelatedObjectsEnum, helper)
    .DoOrder(criteria.ListOrderOptionsEnum, helper);

var result = await rows.OnSelectListWithPaginationAsync(
    criteria.PaginationInfo,
    criteria.ListRelatedObjectsEnum,
    helper,
    CancellationToken.None);

Console.WriteLine($"Rows: {result.Rows.Count}; Total rows: {result.PaginationInfo.TotalRowsCount}; Total pages: {result.PaginationInfo.TotalPagesCount}");
foreach (var product in result.Rows)
    Console.WriteLine($"{product.Id}: {product.Name} ({product.PriceCents} cents), category: {product.CategoryName}");
```

The query starts with `AsNoTracking()`. The filter, include and order steps produce a query; `OnSelectListWithPaginationAsync` then counts the filtered rows and materializes only the requested page. Do not call `Pagination` before this final helper: that would make the count describe only an already restricted query. The cancellation-token overload passes the token to count and materialization.

Expected output:

```text
Rows: 2; Total rows: 3; Total pages: 2
2: Blue pen (900 cents), category: Pens
3: Red pen (900 cents), category: Pens
```

The active minimum-price filter leaves three products. Price descending puts both 900-cent pens first; ID ascending makes their order deterministic. Page 1 returns those two, while `TotalRowsCount` remains 3 and `TotalPagesCount` is 2.

## Behavior and compatibility

Create a criteria instance per request with `ListCriteriaDto<TFilter,TOrder,TInclude>.CreateDefault()`. Its pagination, order and related-object lists initially are null, so assign pagination explicitly when bounded results are required. Null or nonpositive page index/size is treated as unbounded. Empty results have zero pages; nonempty unbounded results have one. The legacy `ListCriteriaDto.Default` field is shared and mutable; `GetCriteriaDto<TInclude>.Default` is a fresh-instance property. `StaticValueDto.PageSize` does not set request pagination.

`DoOrder` passes through null or empty options and ignores null mappings. `OrderColumn.CreateOrder` infers ascending order from an enum name ending in `Asc`; other names mean descending. The package does not choose a tie-break column for you. For bounded pagination, offsets above `Int32.MaxValue` throw `ArgumentOutOfRangeException` with parameter name `pagination`, even for an empty query. The older async pagination overload without a final `CancellationToken` remains available.

The example uses `ISearchableCriteriaSingleTenant<Product,ProductFilter>`. Your application must apply its own authorization and tenant scoping before this query. For a helper that receives a client ID, implement `ISearchableCriteria<TEntity,TFilter>` and call the corresponding `DoSearchCriteria(filter, clientId, helper)` overload. The package does not provide tenant isolation or an EF provider.

This sample was run with the public EF extensions 3.1.0 archive and SQLite 10.0.12. The package's earlier release qualification used EF Core 10.0.12 with PostgreSQL/Npgsql 10.0.3; neither check establishes behavior for every provider or patch. The frontend criteria package has distinct DTO types and is not a dependency of this EF version. Applications on older frameworks need an explicit migration.

The public 3.1.0 NuGet archive contains its earlier embedded README; this guide reflects current repository documentation. Repository source is [MIT licensed](../LICENSE), and the published 3.1.0 archive also carries an MIT expression and license file. See [all published versions and source provenance](../docs/package-versions.md) and the [3.1.0 verification record](../docs/development/RELEASE_3_1_0_VERIFICATION.md).
