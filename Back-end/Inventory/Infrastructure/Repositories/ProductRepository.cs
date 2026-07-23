using System.Linq.Expressions;
using Inventory.Core.Interfaces;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Models.Inventory;

namespace Inventory.Infrastructure.Repositories;

public class ProductRepository(InventoryDbContext context) : IProductRepository
{
    /// <summary>
    /// Asynchronously creates a new product in the database.
    /// </summary>
    /// <param name="product">The product entity to be added to the database.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task CreateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        context.Products.Add(product);

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously retrieves a product from the database by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the product to retrieve.</param>
    /// <param name="asNoTracking">A flag indicating whether the query should use no-tracking mode for read-only operations.</param>
    /// <param name="includeRelatedEntities">A flag indicating whether to include related entities in the query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the product if found, otherwise null.</returns>
    public async Task<Product?> GetProductAsync(Guid id, bool asNoTracking, bool includeRelatedEntities,
                                                CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = context.Products.AsQueryable();

        if (asNoTracking)
        {
            query = query.AsTracking();
        }

        if (includeRelatedEntities)
        {
            query = query.Include(p => p.Category).Include(p => p.Colour).Include(p => p.Brand);
        }

        query = query.Where(b => b.Id == id);

        Product? product = await query.FirstOrDefaultAsync(cancellationToken);

        return product;
    }

    /// <summary>
    /// Asynchronously retrieves a paginated list of products and the total item count from the database.
    /// </summary>
    /// <param name="active">Optional. A boolean value indicating whether to filter by active status. If null, both active and inactive products are considered.</param>
    /// <param name="includeInactive">Optional. A boolean value indicating whether to include inactive products when the active parameter is not specified.</param>
    /// <param name="pageNumber">The page number for the pagination.</param>
    /// <param name="pageSize">The number of products to include on each page.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The result contains a tuple where the first element is a read-only collection of products,
    /// and the second element is the total number of products.
    /// </returns>
    public async Task<(IReadOnlyCollection<Product> items, int itemCount)> GetAllProductsAsync(
        bool? active, bool? includeInactive, int pageNumber, int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = context.Products.AsNoTracking().AsQueryable();

        int itemCount = await query.CountAsync(cancellationToken);

        if (active.HasValue)
        {
            query = query.Where(c => c.IsActive == active.Value);
        }
        else if (!includeInactive.GetValueOrDefault())
        {
            query = query.Where(c => c.IsActive);
        }

        query = query.OrderBy(c => c.Id)
                     .Skip((pageNumber - 1) * pageSize)
                     .Take(pageSize);

        IReadOnlyCollection<Product> categoryList = await query.ToListAsync(cancellationToken);

        return (categoryList, itemCount);
    }

    /// <summary>
    /// Asynchronously retrieves a collection of products that match the specified criteria.
    /// </summary>
    /// <param name="predicate">A filter expression to match the desired products.</param>
    /// <param name="asNoTracking">Indicates whether the query should be executed with no tracking.</param>
    /// <param name="includeRelatedEntities">Determines if related entities should be included in the query.</param>
    /// <param name="pageNumber">The page number for paginated results. Defaults to 1.</param>
    /// <param name="pageSize">The number of items per page for paginated results. Defaults to 20.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a tuple with a collection of matching products and the total count of products that match the filter.</returns>
    public async Task<(IReadOnlyCollection<Product> Items, int TotalCount)> GetProductsAsync(
        Expression<Func<Product, bool>> predicate,
        bool asNoTracking, bool includeRelatedEntities, int? pageNumber = 1,
        int? pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = context.Products.AsQueryable();

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        if (includeRelatedEntities)
        {
            query = query.Include(p => p.Category).Include(p => p.Colour).Include(p => p.Brand);
        }

        query = query.Where(predicate);

        int matchesFound = await query.CountAsync(cancellationToken);

        if (pageNumber != null && pageSize != null)
        {
            query = query.OrderBy(p => p.Id)
                         .Skip((pageNumber.Value - 1) * pageSize.Value)
                         .Take(pageSize.Value);
        }

        IReadOnlyCollection<Product> productList = await query.ToListAsync(cancellationToken);

        return (productList, matchesFound);
    }

    /// <summary>
    /// Asynchronously saves all pending changes to the database context.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete, allowing the operation to be cancelled.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}