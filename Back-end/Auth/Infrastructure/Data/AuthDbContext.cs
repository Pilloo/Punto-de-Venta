using Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Core.Domain;

namespace Infrastructure.Data
{
    public class AuthDbContext(DbContextOptions<AuthDbContext> options) : IdentityDbContext<User>
    {
        public DbSet<User> Users { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>().Ignore(x => x.Email);
            builder.Entity<User>().Ignore(x => x.EmailConfirmed);
            builder.Entity<User>().Ignore(x => x.NormalizedEmail);
            builder.Entity<User>().Ignore(x => x.LockoutEnabled);
            builder.Entity<User>().Ignore(x => x.LockoutEnd);
            builder.Entity<User>().Ignore(x => x.TwoFactorEnabled);
            builder.Entity<User>().Ignore(x => x.PhoneNumber);
            builder.Entity<User>().Ignore(x => x.PhoneNumberConfirmed);
        }
    }
}
