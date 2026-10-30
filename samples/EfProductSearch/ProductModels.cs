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
