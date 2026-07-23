using System.Linq.Expressions;
using Inventory.Core.Interfaces;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Models.Inventory;

namespace Inventory.Infrastructure.Repositories;

public class BrandRepository(InventoryDbContext context) : IBrandRepository
{
    /// <summary>
    /// Asynchronously creates a new brand in the database.
    /// </summary>
    /// <param name="brand">The brand entity to be added to the database.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task CreateBrandAsync(Brand brand, CancellationToken cancellationToken = default)
    {
        context.Brands.Add(brand);

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously retrieves a brand from the database by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the brand to retrieve.</param>
    /// <param name="asNoTracking">Determines whether the retrieved entity should be tracked by the database context.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the requested brand if found; otherwise, null.</returns>
    public async Task<Brand?> GetBrandAsync(Guid id, bool asNoTracking, CancellationToken cancellationToken = default)
    {
        IQueryable<Brand> query = context.Brands.AsQueryable();

        if (asNoTracking)
        {
            query = query.AsTracking();
        }

        query = query.Where(b => b.Id == id);

        Brand? brand = await query.FirstOrDefaultAsync(cancellationToken);

        return brand;
    }

    /// <summary>
    /// Asynchronously retrieves a paginated list of all brands from the database, along with the total brand count.
    /// </summary>
    /// <param name="active">A boolean value to filter brands based on active status. If null, no filtering is applied.</param>
    /// <param name="includeInactive">A boolean value indicating whether to include inactive brands if no specific filter is applied.</param>
    /// <param name="pageNumber">The number of the page to retrieve. Defaults to 1.</param>
    /// <param name="pageSize">The number of brands per page. Defaults to 10.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task which represents the asynchronous operation, returning a tuple containing a list of brands and the total count of brands.</returns>
    public async Task<(IReadOnlyList<Brand> items, int itemCount)> GetAllBrandsAsync(
        bool? active, bool? includeInactive, int pageNumber = 1, int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Brand> query = context.Brands.AsNoTracking().AsQueryable();

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

        IReadOnlyList<Brand> brandList = await query.ToListAsync(cancellationToken);

        return (brandList, itemCount);
    }

    /// <summary>
    /// Asynchronously retrieves a collection of brands from the database based on the specified criteria.
    /// </summary>
    /// <param name="predicate">A predicate expression used to filter the brands.</param>
    /// <param name="asNoTracking">A boolean indicating whether the query should be executed with no tracking.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task, which represents the asynchronous operation, containing a collection of brands that match the criteria.</returns>
    public async Task<IEnumerable<Brand?>> GetBrandsAsync(Expression<Func<Brand, bool>> predicate,
                                                          bool asNoTracking,
                                                          CancellationToken cancellationToken = default)
    {
        IQueryable<Brand> query = context.Brands.AsQueryable();

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        IEnumerable<Brand?> brandList = await query.Where(predicate).ToListAsync(cancellationToken);

        return brandList;
    }

    /// <summary>
    /// Asynchronously updates an existing brand in the database.
    /// </summary>
    /// <param name="brand">The brand entity to be updated in the database.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task UpdateBrandAsync(Brand brand, CancellationToken cancellationToken = default)
    {
        context.Brands.Update(brand);

        await context.SaveChangesAsync(cancellationToken);
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