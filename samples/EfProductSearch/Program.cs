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
