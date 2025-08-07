using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectScene.Domain.Interfaces;
using ProjectScene.Infrastructure.Data;
using ProjectScene.Infrastructure.Repositories;

namespace ProjectScene.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Aqui você configura:
            // - Banco de dados (DbContext)
            // - Repositórios
            // - Serviços externos (APIs, Storage, etc.)

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}
