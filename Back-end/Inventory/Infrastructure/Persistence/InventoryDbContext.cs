using EntityFramework.Exceptions.SqlServer;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Inventory.Infrastructure.Persistence
{
    public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
    {
        public DbSet<Colour> Colours { get; set; }
        
        public DbSet<Brand> Brands { get; set; }
       
        public DbSet<Category> Categories { get; set; }
             
        public DbSet<Product> Products { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.UseCollation("Latin1_General_100_CI_AS");
        }
    }
}
