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
