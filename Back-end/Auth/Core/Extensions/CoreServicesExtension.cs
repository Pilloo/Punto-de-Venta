using AuthModule.Core.Pipelines;
using AuthModule.Core.Features;
using ErrorHandling;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using DTOs.Auth;

namespace AuthModule.Core.Extensions
{
    public static class CoreServicesExtension
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(
                [
                    Assembly.GetExecutingAssembly()
                ]);
                cfg.AddBehavior<IPipelineBehavior<ModifyUserCommand, Result<TokenResponse>>, ModifyUserValidationPipeline>();
            });

            return services;
        }
    }
}
