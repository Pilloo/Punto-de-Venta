using AuthModule.Core.Interfaces;
using AuthModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Linq.Expressions;

namespace AuthModule.Infrastructure.Repositories
{
    /// <summary>
    /// Provides data access operations for user entities in the authentication system.
    /// </summary>
    /// <remarks>
    /// This repository implements the <see cref="IUserRepository"/> interface and encapsulates
    /// database queries related to user management using Entity Framework Core.
    /// </remarks>
    public class UserRepository(AuthDbContext context) : IUserRepository
    {
        public async Task<IEnumerable<User?>> GetUsersAsync(Expression<Func<User, bool>> predicate, bool asNoTracking, CancellationToken cancellationToken)
        {
            try
            {
                IQueryable<User> query = context.Users.IgnoreQueryFilters().Where(predicate);

                if (asNoTracking)
                {
                    query.AsNoTracking();
                }

                IEnumerable<User> userList = await query.ToListAsync(cancellationToken);

                return userList;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Retrieves a single user matching the specified predicate.
        /// </summary>
        /// <param name="predicate">
        /// A lambda expression that defines the filtering criteria to locate the desired user
        /// (e.g., <c>u => u.Email == "user@example.com"</c>).
        /// </param>
        /// <param name="asNoTracking">
        /// If <c>true</c>, uses no-tracking mode to improve performance for read-only operations.
        /// </param>
        /// <param name="ct">Cancellation token for the asynchronous operation.</param>
        /// <returns>
        /// The <see cref="User"/> entity matching the predicate, or <c>null</c> if no matching user is found.
        /// </returns>
        /// <remarks>
        /// Ignores global query filters and returns only the first match if multiple users satisfy the predicate.
        /// </remarks>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the cancellation token is cancelled before the operation completes.
        /// </exception>
        public async Task<User?> GetUserAsync(Expression<Func<User, bool>> predicate, bool asNoTracking, CancellationToken ct)
        {
            try
            {
                IQueryable<User> query = context.Users.IgnoreQueryFilters().Where(predicate);

                if (asNoTracking)
                {
                    query.AsNoTracking();
                }

                User? result = await query.FirstOrDefaultAsync(ct);

                return result;
            }
            catch
            {
                throw;
            }
        }

        public async Task SaveChangesAsync(CancellationToken ct)
        {
            await context.SaveChangesAsync(ct);
        }
    }
}
