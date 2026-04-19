using Microsoft.Extensions.DependencyInjection;
using ProjectScene.Application.Interfaces;
using ProjectScene.Application.Services;

namespace ProjectScene.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Registra os casos de uso da camada de aplicação.
            services.AddScoped<IUserService, UserService>();
            return services;
        }
    }
}
