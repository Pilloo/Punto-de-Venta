using AuthModule.Core.Interfaces;
using AuthModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Models.Auth;

namespace AuthModule.Infrastructure.Repositories
{
    /// <summary>
    /// The UserRepository class provides methods to interact with the user data in the persistence layer.
    /// It serves as the implementation for the IUserRepository interface and operates on the AuthDbContext.
    /// </summary>
    public class UserRepository(AuthDbContext context) : IUserRepository
    {
        /// <summary>
        /// Retrieves a paginated list of users that match the specified criteria.
        /// </summary>
        /// <param name="predicate">
        /// A lambda expression that defines the filtering criteria to identify the desired users.
        /// </param>
        /// <param name="asNoTracking">
        /// If <c>true</c>, the query will use no-tracking mode to optimise performance for read-only operations.
        /// </param>
        /// <param name="pageNumber">
        /// The page number to retrieve, used for paginated results. Defaults to 1.
        /// </param>
        /// <param name="pageSize">
        /// The number of users per page, used for paginated results. Defaults to 10.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token for the asynchronous operation.
        /// </param>
        /// <returns>
        /// A tuple containing the following:
        /// - An <see cref="IReadOnlyCollection{User}"/> representing the list of users that match the specified filter.
        /// - An <see cref="int"/> representing the total count of users that satisfy the filtering criteria.
        /// </returns>
        /// <remarks>
        /// This method supports pagination and no-tracking optimisations. The total count of users matching the
        /// filtering criteria is also included in the result to aid in paginated display.
        /// </remarks>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the operation is cancelled via the provided cancellation token.
        /// </exception>
        public async Task<(IReadOnlyCollection<User> users, int userCount)> GetUsersAsync(
            Expression<Func<User, bool>> predicate,
            bool asNoTracking = true,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            IQueryable<User> query = context.Users.Where(predicate);

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            int userCount = await query.CountAsync(cancellationToken);

            query = query.OrderBy(u => u.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize);

            IReadOnlyCollection<User> userList = await query.ToListAsync(cancellationToken);

            return (userList, userCount);
        }

        /// <summary>
        /// Retrieves a paginated list of all users, with optional filtering based on their active status.
        /// </summary>
        /// <param name="active">
        /// An optional parameter that, if specified, filters the users based on their active status
        /// (<c>true</c> for active users, <c>false</c> for inactive users).
        /// </param>
        /// <param name="includeInactive">
        /// An optional parameter that, if set to <c>true</c>, includes inactive users in the results
        /// when <paramref name="active"/> is not specified. Defaults to <c>false</c>.
        /// </param>
        /// <param name="pageNumber">
        /// The page number to retrieve, used for paginated results. Defaults to 1.
        /// </param>
        /// <param name="pageSize">
        /// The number of users per page to include in the results. Defaults to 10.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token for the asynchronous operation.
        /// </param>
        /// <returns>
        /// A tuple containing the following:
        /// - An <see cref="IReadOnlyCollection{User}"/> representing the users retrieved from the database.
        /// - An <see cref="int"/> representing the total count of users in the database, prior to pagination.
        /// </returns>
        /// <remarks>
        /// This method uses no-tracking mode to optimise performance for read-only operations.
        /// When filtering by active status, the most precise filter is applied based on the provided arguments.
        /// The result supports pagination to limit the volume of returned data.
        /// </remarks>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the operation is cancelled via the provided cancellation token.
        /// </exception>
        public async Task<(IReadOnlyCollection<User> users, int userCount)> GetAllUsersAsync(
            bool? active = null,
            bool? includeInactive = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            IQueryable<User> query = context.Users.AsNoTracking().AsQueryable();

            int userCount = await query.CountAsync(cancellationToken);

            if (active.HasValue)
            {
                query = query.Where(u => u.IsActive == active.Value);
            }
            else if (!includeInactive.GetValueOrDefault())
            {
                query = query.Where(u => u.IsActive);
                
            }
            
            query = query.OrderBy(u => u.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize);

            IReadOnlyCollection<User> userList = await query.ToListAsync(cancellationToken);

            return (userList, userCount);
        }

        /// <summary>
        /// Retrieves a single user that matches the specified criteria.
        /// </summary>
        /// <param name="predicate">
        /// A lambda expression that specifies the filtering criteria to identify the desired user.
        /// </param>
        /// <param name="asNoTracking">
        /// If <c>true</c>, uses no-tracking mode to optimise performance for read-only operations.
        /// </param>
        /// <param name="includeInactive">
        /// If <c>true</c>, includes inactive users in the search; otherwise, only active users are considered.
        /// </param>
        /// <param name="ct">
        /// Cancellation token for the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="User"/> object that matches the specified criteria, or <c>null</c> if no user is found.
        /// </returns>
        /// <remarks>
        /// The method supports filtering active or inactive users based on the <paramref name="includeInactive"/> parameter and leverages no-tracking mode for read-only optimisations.
        /// </remarks>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the operation is cancelled via the provided cancellation token.
        /// </exception>
        public async Task<User?> GetUserAsync(Expression<Func<User, bool>> predicate, bool asNoTracking = true,
                                              bool includeInactive = false, CancellationToken ct = default)
        {
            IQueryable<User> query = context.Users.Where(predicate);

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            if (!includeInactive)
            {
                query = query.Where(u => u.IsActive);
            }

            User? result = await query.FirstOrDefaultAsync(ct);

            return result;
        }

        public async Task SaveChangesAsync(CancellationToken ct)
        {
            await context.SaveChangesAsync(ct);
        }
    }
}