using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class InfrastructureServicesExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        ConfigurationManager configuration)
    {
        services.AddDbContext<AuthDbContext>(
            options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("AuthDb"));
            });

        services.AddTransient<IIdentityService, IdentityService>();
        services.AddTransient<ICryptoService, CryptoService>();
        services.AddTransient<IRefreshTokenRepository, RefreshTokenRepository>();

        return services;
    }
}
