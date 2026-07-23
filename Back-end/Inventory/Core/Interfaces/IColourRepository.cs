using System.Linq.Expressions;
using Models;
using Models.Inventory;

namespace Inventory.Core.Interfaces;

public interface IColourRepository
{
    public Task CreateColourAsync(Colour colour, CancellationToken cancellationToken = default);

    public Task<(IReadOnlyList<Colour> items, int itemCount)> GetAllColoursAsync(
        bool? active, bool? includeInactive, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    public Task<IEnumerable<Colour?>> GetColoursAsync(Expression<Func<Colour, bool>> predicate,
                                                      bool asNoTracking,
                                                      CancellationToken cancellationToken = default);

    public Task<Colour?> GetColourAsync(Guid id, bool asNoTracking, CancellationToken cancellationToken = default);
    public Task UpdateColourAsync(Colour colour, CancellationToken cancellationToken = default);
    public Task SaveChangesAsync(CancellationToken cancellationToken = default);
}