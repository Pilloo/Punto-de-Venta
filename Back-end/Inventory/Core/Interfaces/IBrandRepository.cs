using System.Linq.Expressions;
using Models;

namespace Inventory.Core.Interfaces;

public interface IBrandRepository
{
    public Task CreateBrandAsync(Brand brand, CancellationToken cancellationToken = default);

    public Task<(IReadOnlyList<Brand> items, int itemCount)> GetAllBrandsAsync(bool? active, bool? includeInactive,
                                                                               int pageNumber, int pageSize,
                                                                               CancellationToken cancellationToken =
                                                                                   default);

    public Task<IEnumerable<Brand?>> GetBrandsAsync(Expression<Func<Brand, bool>> predicate,
                                                    bool asNoTracking,
                                                    CancellationToken cancellationToken = default);

    public Task<Brand?> GetBrandAsync(Guid id, bool asNoTracking, CancellationToken cancellationToken = default);
    public Task UpdateBrandAsync(Brand brand, CancellationToken cancellationToken = default);
    public Task SaveChangesAsync(CancellationToken cancellationToken = default);
}