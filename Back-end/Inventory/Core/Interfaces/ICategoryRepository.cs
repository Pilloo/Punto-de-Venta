using System.Linq.Expressions;
using Models;
using Models.Inventory;

namespace Inventory.Core.Interfaces;

public interface ICategoryRepository
{
    public Task CreateCategoryAsync(Category category, CancellationToken cancellationToken = default);

    public Task<(IReadOnlyList<Category> Items, int ItemCount)> GetAllCategoriesAsync(
        bool? active, bool? includeInactive, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    public Task<IEnumerable<Category>> GetCategoriesAsync(Expression<Func<Category, bool>> predicate,
                                                          bool asNoTracking,
                                                          CancellationToken cancellationToken = default);

    public Task<Category?> GetCategoryAsync(Guid id, bool asNoTracking, CancellationToken cancellationToken = default);
    public Task UpdateCategoryAsync(Category category, CancellationToken cancellationToken = default);
    public Task SaveChangesAsync(CancellationToken cancellationToken = default);
}