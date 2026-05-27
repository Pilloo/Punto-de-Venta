using Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace AuthModule.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User?>> GetUsersAsync(Expression<Func<User, bool>> predicate, bool asNoTracking, CancellationToken ct);
     
        Task<User?> GetUserAsync(Expression<Func<User, bool>> predicate, bool asNoTracking, CancellationToken ct);

        Task SaveChangesAsync(CancellationToken ct);
    }
}
