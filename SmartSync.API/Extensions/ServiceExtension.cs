using SmartSync.Application.Interfaces;
using SmartSync.Application.Services;

namespace SmartSync.API.Extensions
{
    internal static class ServiceExtension
    {
        internal static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            services.AddTransient<IUsuarioService, UsuarioService>();
            services.AddTransient<IDispositivoService, DispositivoService>();
            services.AddTransient<IComodoService, ComodoService>();
            services.AddTransient<IResidenciaService, ResidenciaService>();
            services.AddTransient<ITipoDispositivoService, TipoDispositivoService>();

            return services;
        }
    }
}
