using Inventory.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Infrastructure.Extensions
{
    public static class InfrastructureServicesExtension
    {
        public static WebApplicationBuilder AddInfrastructureService(this WebApplicationBuilder? builder)
        {
            builder!.AddSqlServerDbContext<InventoryDbContext>(connectionName: "inventory-database");

            return builder!;
        }
    }
}
