using System.Linq.Expressions;
using Inventory.Core.Interfaces;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Inventory.Infrastructure.Repositories;

public class ColourRepository(InventoryDbContext context) : IColourRepository
{
    /// <summary>
    /// Asynchronously creates a new colour in the database.
    /// </summary>
    /// <param name="colour">The colour entity to be added.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task CreateColourAsync(Colour colour, CancellationToken cancellationToken = default)
    {
        context.Colours.Add(colour);

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously retrieves a colour from the database based on the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the colour to retrieve.</param>
    /// <param name="asNoTracking">A boolean value indicating whether the retrieved entity should not be tracked by the context.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the colour entity if found, otherwise null.</returns>
    public async Task<Colour?> GetColourAsync(Guid id, bool asNoTracking, CancellationToken cancellationToken = default)
    {
        IQueryable<Colour> query = context.Colours.AsQueryable();

        if (asNoTracking)
        {
            query = query.AsTracking();
        }

        query = query.Where(c => c.Id == id);

        Colour? colour = await query.FirstOrDefaultAsync(cancellationToken);

        return colour;
    }

    /// <summary>
    /// Asynchronously retrieves all colours based on specified filters and pagination parameters.
    /// </summary>
    /// <param name="active">Specifies whether to filter colours by their active status. If null, no filtering is applied.</param>
    /// <param name="includeInactive">Indicates whether inactive colours should be included when no specific active status is applied.</param>
    /// <param name="pageNumber">The page number for pagination.</param>
    /// <param name="pageSize">The number of items to include per page.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing a tuple with the list of colours and the total number of items.</returns>
    public async Task<(IReadOnlyList<Colour> items, int itemCount)> GetAllColoursAsync(
        bool? active, bool? includeInactive, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        IQueryable<Colour> query = context.Colours.AsNoTracking().AsQueryable();

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

        IReadOnlyList<Colour> colourList = await query.ToListAsync(cancellationToken);

        return (colourList, itemCount);
    }

    /// <summary>
    /// Asynchronously retrieves a collection of colours from the database that match a specified predicate.
    /// </summary>
    /// <param name="predicate">A lambda expression used to filter the colours.</param>
    /// <param name="asNoTracking">Indicates whether the query should be tracked by the Entity Framework change tracker.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the collection of matching colours.</returns>
    public async Task<IEnumerable<Colour?>> GetColoursAsync(Expression<Func<Colour, bool>> predicate, bool asNoTracking,
                                                            CancellationToken cancellationToken = default)
    {
        IQueryable<Colour> query = context.Colours.AsQueryable();

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        IEnumerable<Colour?> colourList = await query.Where(predicate).ToListAsync(cancellationToken);

        return colourList;
    }

    /// <summary>
    /// Asynchronously updates an existing colour in the database.
    /// </summary>
    /// <param name="colour">The colour entity to be updated.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateColourAsync(Colour colour, CancellationToken cancellationToken = default)
    {
        context.Colours.Update(colour);

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously saves all changes made in the context to the database.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}