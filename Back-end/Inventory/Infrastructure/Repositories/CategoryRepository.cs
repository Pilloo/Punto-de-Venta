using System.Linq.Expressions;
using Inventory.Core.Interfaces;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Inventory.Infrastructure.Repositories;

public class CategoryRepository(InventoryDbContext context) : ICategoryRepository
{
    /// <summary>
    /// Asynchronously creates a new category in the database.
    /// </summary>
    /// <param name="category">The <see cref="Category"/> object to be created.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task CreateCategoryAsync(Category category, CancellationToken cancellationToken = default)
    {
        context.Categories.Add(category);

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously retrieves a category from the database based on the provided identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the category to retrieve.</param>
    /// <param name="asNoTracking">A boolean value indicating whether to retrieve the category without tracking its changes in the context.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, with the result being the <see cref="Category"/> instance if found, or null if not found.</returns>
    public async Task<Category?> GetCategoryAsync(Guid id, bool asNoTracking,
                                                  CancellationToken cancellationToken = default)
    {
        IQueryable<Category> query = context.Categories.AsQueryable();

        if (asNoTracking)
        {
            query = query.AsTracking();
        }

        query = query.Where(c => c.Id == id);

        Category? category = await query.FirstOrDefaultAsync(cancellationToken);

        return category;
    }

    /// <summary>
    /// Asynchronously retrieves a paginated list of categories from the database.
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve, where the first page is 1.</param>
    /// <param name="pageSize">The number of categories to retrieve per page.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation. The result contains a tuple where the first item is the collection of categories and the second item is the total count of categories.</returns>
    public async Task<(IReadOnlyList<Category> Items, int ItemCount)> GetAllCategoriesAsync(
        bool? active, bool? includeInactive, int pageNumber = 1, int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Category> query = context.Categories.AsNoTracking().AsQueryable();

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

        IReadOnlyList<Category> categoryList = await query.ToListAsync(cancellationToken);

        return (categoryList, itemCount);
    }

    /// <summary>
    /// Asynchronously retrieves a collection of categories that match the specified predicate.
    /// </summary>
    /// <param name="predicate">The filter criteria as an expression used to select specific categories.</param>
    /// <param name="asNoTracking">Indicates whether the entities should be retrieved without tracking changes.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing an enumerable of matching <see cref="Category"/> objects.</returns>
    public async Task<IEnumerable<Category>> GetCategoriesAsync(Expression<Func<Category, bool>> predicate,
                                                                bool asNoTracking,
                                                                CancellationToken cancellationToken = default)
    {
        IQueryable<Category> query = context.Categories.AsQueryable();

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        IEnumerable<Category> categoryList = await query.Where(predicate).ToListAsync(cancellationToken);

        return categoryList;
    }

    /// <summary>
    /// Asynchronously updates an existing category in the database.
    /// </summary>
    /// <param name="category">The <see cref="Category"/> object to be updated.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task UpdateCategoryAsync(Category category, CancellationToken cancellationToken = default)
    {
        context.Categories.Update(category);

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously saves all changes made in the context to the database.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous save operation.</returns>
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}