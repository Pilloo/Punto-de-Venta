using Microsoft.Extensions.DependencyInjection;
using Services.CryptoService.Interface;

namespace Services.CryptoService.Extensions;

public static class CryptoServiceExtension
{
    public static IServiceCollection AddCryptoService(this IServiceCollection services)
    {
        services.AddTransient<ICryptoService, Implementation.CryptoService>();

        return services;
    }
}