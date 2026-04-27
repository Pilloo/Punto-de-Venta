using Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Core.Domain;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Data
{
    public class AuthDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>().Ignore(x => x.Email);
            builder.Entity<User>().Ignore(x => x.EmailConfirmed);
            builder.Entity<User>().Ignore(x => x.NormalizedEmail);
            builder.Entity<User>().Ignore(x => x.LockoutEnabled);
            builder.Entity<User>().Ignore(x => x.LockoutEnd);
            builder.Entity<User>().Ignore(x => x.AccessFailedCount);
            builder.Entity<User>().Ignore(x => x.TwoFactorEnabled);
            builder.Entity<User>().Ignore(x => x.PhoneNumber);
            builder.Entity<User>().Ignore(x => x.PhoneNumberConfirmed);

            builder.Entity<RefreshToken>()
                   .HasOne<User>()
                   .WithMany()
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
