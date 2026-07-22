using EntityFramework.Exceptions.SqlServer;
using Inventory.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Inventory.Core.Interfaces;
using Inventory.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Infrastructure.Extensions
{
    public static class InfrastructureServicesExtension
    {
        public static WebApplicationBuilder AddInfrastructureService(this WebApplicationBuilder? builder)
        {
            builder!.AddSqlServerDbContext<InventoryDbContext>(connectionName: "inventory-database",
                                                               configureDbContextOptions: options =>
                                                               {
                                                                   options.UseExceptionProcessor();
                                                               });

            builder!.Services.AddTransient<IBrandRepository, BrandRepository>();
            builder!.Services.AddTransient<ICategoryRepository, CategoryRepository>();
            builder!.Services.AddTransient<IColourRepository, ColourRepository>();
            builder!.Services.AddTransient<IProductRepository, ProductRepository>();

            return builder!;
        }
    }
}