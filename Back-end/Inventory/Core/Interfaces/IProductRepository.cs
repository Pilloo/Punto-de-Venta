using System.Linq.Expressions;
using Models;

namespace Inventory.Core.Interfaces;

public interface IProductRepository
{
    public Task CreateProductAsync(Product product, CancellationToken cancellationToken = default);

    public Task<Product?> GetProductAsync(Guid id, bool asNoTracking, bool includeRelatedEntities,
                                          CancellationToken cancellationToken = default);

    public Task<(IReadOnlyCollection<Product> items, int itemCount)> GetAllProductsAsync(
        bool? active, bool? includeInactive, int pageNumber, int pageSize,
        CancellationToken cancellationToken = default);

    public Task<(IReadOnlyCollection<Product> Items, int TotalCount)> GetProductsAsync(
        Expression<Func<Product, bool>> predicate,
        bool asNoTracking,
        bool includeRelatedEntities,
        int? pageNumber, int? pageSize,
        CancellationToken cancellationToken = default);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default);
}