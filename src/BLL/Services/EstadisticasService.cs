using PetFinder.BLL.Dtos;
using PetFinder.DAL.Interfaces;

namespace PetFinder.BLL.Services;

/// <summary>Calcula los indicadores de impacto del dominio sin introducir cifras inventadas.</summary>
public sealed class EstadisticasService(
    IRefugioRepository refugioRepository,
    IMascotaRepository mascotaRepository,
    IReporteRepository reporteRepository) : IEstadisticasService
{
    /// <inheritdoc />
    public async Task<EstadisticasImpactoDto> ObtenerAsync(CancellationToken cancellationToken = default)
    {
        var refugios = await refugioRepository.ObtenerTodosAsync(soloActivos: true, cancellationToken);
        var mascotas = await mascotaRepository.ObtenerTodasAsync(cancellationToken);
        var reportes = await reporteRepository.ObtenerTodosAsync(cancellationToken);

        return new EstadisticasImpactoDto(
            MascotasReunidas: reportes.Count(reporte => reporte.MascotaReunida),
            RefugiosAliados: refugios.Count,
            Adopciones: mascotas.Count(mascota => mascota.Adoptada),
            ReportesActivos: reportes.Count(reporte => reporte.Activo));
    }
}
