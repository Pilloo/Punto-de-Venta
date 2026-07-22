using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Core.Extensions;

public static class CoreServicesExtension
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies([
                Assembly.GetExecutingAssembly()
            ]);
        });

        return services;
    }
}