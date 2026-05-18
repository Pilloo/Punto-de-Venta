using AuthModule.Core.Interfaces;
using AuthModule.Infrastructure.Persistence;
using AuthModule.Infrastructure.Repositiories;
using AuthModule.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthModule.Infrastructure.Extensions;

public static class InfrastructureServicesExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        ConfigurationManager configuration)
    {
        services.AddDbContext<AuthDbContext>(
            options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("AuthDataBase"));
            });

        services.AddTransient<IIdentityService, IdentityService>();
        services.AddTransient<ICryptoService, CryptoService>();
        services.AddTransient<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddTransient<IUserRepository, UserRepository>();
        
        return services;
    }
}
