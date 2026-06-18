#nullable enable
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Data;
using Warehouse.Repositories.Implementations;
using Warehouse.Repositories.Interfaces;
using Warehouse.Services.Interfaces;
using Warehouse.Services.Implementations;

namespace Warehouse.Helpers
{
    public static class ServiceRegistration
    {
        public static void RegisterApplicationServices(IServiceCollection services, IConfiguration configuration)
        {
            // Generic repository interfaces
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUnitOfWorkAsync, UnitOfWorkAsync>();

            // UnitOfWork service
            services.AddScoped<IUnitOfWorkService, UnitOfWorkService>();

            // Auth & JWT services
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IAuthService, AuthService>();
        }
    }
}
