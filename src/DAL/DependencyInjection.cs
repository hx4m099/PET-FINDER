using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PetFinder.DAL.Interfaces;
using PetFinder.DAL.Repositories;

namespace PetFinder.DAL;

/// <summary>Registro de servicios de acceso a datos.</summary>
public static class DependencyInjection
{
    /// <summary>Registra los repositorios en memoria y su almacén compartido.</summary>
    public static IServiceCollection AddDal(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<InMemoryDataStore>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IRefugioRepository, RefugioRepository>();
        services.AddScoped<IMascotaRepository, MascotaRepository>();
        services.AddScoped<IReporteRepository, ReporteRepository>();
        services.AddScoped<ICitaRepository, CitaRepository>();
        return services;
    }
}
