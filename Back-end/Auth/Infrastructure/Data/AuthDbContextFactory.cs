using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Data;

public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();

        // Use a dummy connection string or a real one from an environment variable.
        // The tools only need this to "shape" the migration, not to actually connect 
        // to a live DB right this second.
        optionsBuilder.UseSqlServer("Server = 127.0.0.1,1433; Database=auth_service; User = auth_service; Password = u!o!9Nw^oIlLO*dw; TrustServerCertificate = true; MultipleActiveResultSets = True");

        return new AuthDbContext(optionsBuilder.Options);
    }
}