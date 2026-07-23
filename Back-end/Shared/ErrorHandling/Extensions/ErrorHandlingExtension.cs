using ErrorHandling.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ErrorHandling.Extensions
{
    public static class ErrorHandlingExtension
    {
        public static IServiceCollection AddErrorHadlingService(this IServiceCollection services, ConfigurationManager configurationManager)
        {
            services.AddHttpContextAccessor();
            services.AddSingleton<ErrorFactory>();

            return services;
        }
    }
}
