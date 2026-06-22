using AuthModule.Core.Interfaces;
using AuthModule.Infrastructure.Persistence;
using AuthModule.Infrastructure.Repositiories;
using AuthModule.Infrastructure.Repositories;
using AuthModule.Infrastructure.Services;
using Aspire.Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;
using Models;

namespace AuthModule.Infrastructure.Extensions;

public static class InfrastructureServicesExtensions
{
    public static WebApplicationBuilder AddInfrastructureServices(this WebApplicationBuilder? builder)
    {
        builder!.AddSqlServerDbContext<AuthDbContext>(connectionName: "auth-database");

        builder!.Services.AddTransient<IIdentityService, IdentityService>();
        builder!.Services.AddTransient<ICryptoService, CryptoService>();
        builder!.Services.AddTransient<IRefreshTokenRepository, RefreshTokenRepository>();
        builder!.Services.AddTransient<IUserRepository, UserRepository>();
        
        return builder;
    }

    public static async Task SeedIdentityAsync(this WebApplication application)
    {
        using IServiceScope scope = application.Services.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        Guid defaultUserGuid = Guid.Parse("19a1a598-6cc1-4d59-9590-9a2e235bc5df");

        if (await userManager.FindByIdAsync(defaultUserGuid.ToString()) is not null)
        {
            return;
        }

        User defaultUser = new User
        {
            Id = defaultUserGuid,
            UserName = "DefaultUser",
            GivenName = "DefaultUser",
            LastName = "ChangeBeforeProd"
        };

        IdentityResult result = await userManager.CreateAsync(defaultUser, "HB-CxPfPwtU4F48");

        if (!result.Succeeded)
        {
            throw new Exception(result.Errors.FirstOrDefault()!.Description);
        }

        return;
    }
}
