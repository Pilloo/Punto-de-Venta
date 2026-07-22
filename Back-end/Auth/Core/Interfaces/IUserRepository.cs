using Models;
using System.Linq.Expressions;

namespace AuthModule.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<(IReadOnlyCollection<User> users, int userCount)> GetAllUsersAsync(
            bool? active, bool? includeInactive, int pageNumber, int pageSize, CancellationToken ct);


        Task<User?> GetUserAsync(Expression<Func<User, bool>> predicate, bool asNoTracking, bool includeInactive,
                                 CancellationToken ct);

        Task SaveChangesAsync(CancellationToken ct);
    }
}