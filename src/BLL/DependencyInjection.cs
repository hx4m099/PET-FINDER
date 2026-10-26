using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PetFinder.BLL.Dtos;
using PetFinder.BLL.Services;
using PetFinder.BLL.Validators;
using PetFinder.DAL;

namespace PetFinder.BLL;

/// <summary>Registro de servicios de negocio y validadores de PetFinder.</summary>
public static class DependencyInjection
{
    /// <summary>Registra DAL, servicios BLL y validadores de DTO.</summary>
    public static IServiceCollection AddBll(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddDal();
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<IImageSimilarityService, MockImageSimilarityService>();
        services.AddScoped<AuthService>();
        services.AddScoped<ReporteService>();
        services.AddScoped<CoincidenciaService>();
        services.AddScoped<AdopcionService>();
        services.AddScoped<CitaService>();
        services.AddScoped<IValidator<RegistroUsuarioDto>, RegistroUsuarioValidator>();
        services.AddScoped<IValidator<InicioSesionDto>, InicioSesionValidator>();
        services.AddScoped<IValidator<CrearReporteMascotaDto>, CrearReporteMascotaValidator>();
        services.AddScoped<IValidator<BuscarCoincidenciasDto>, BuscarCoincidenciasValidator>();
        services.AddScoped<IValidator<SolicitarCitaDto>, SolicitarCitaValidator>();
        return services;
    }
}
