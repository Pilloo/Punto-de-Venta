using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Infrastructure.Persistence
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }
    
        public DbSet<Colour> Colours { get; set; }
        
        public DbSet<Brand> Brands { get; set; }
       
        public DbSet<Category> Categories { get; set; }
             
        public DbSet<Product> Products { get; set; }
    }
}
