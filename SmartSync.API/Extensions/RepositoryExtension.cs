using SmartSync.Infraestructure.Persistence.Interfaces;
using SmartSync.Infraestructure.Persistence.Repositories;

namespace SmartSync.API.Extensions
{
    internal static class RepositoryExtension
    {
        internal static IServiceCollection ConfigureRepositories(this IServiceCollection services)
        {
            services.AddTransient<IUsuarioRepository, UsuarioRepository>();
            services.AddTransient<IDispositivoRepository, DispositivoRepository>();
            services.AddTransient<IComodoRepository, ComodoRepository>();
            services.AddTransient<IResidenciaRepository, ResidenciaRepository>();
            services.AddTransient<ITipoDispositivoRepository, TipoDispositivoRepository>();

            return services;
        }
    }
}
