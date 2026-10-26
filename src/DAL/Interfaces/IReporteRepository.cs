using PetFinder.Entity.Entities;
using PetFinder.Entity.Enums;

namespace PetFinder.DAL.Interfaces;

/// <summary>Abstracción de persistencia para reportes de mascotas.</summary>
public interface IReporteRepository
{
    /// <summary>Busca un reporte por su identificador.</summary>
    Task<ReporteMascota?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Obtiene candidatos activos filtrando especie, ciudad y fecha antes de comparar imágenes.</summary>
    Task<IReadOnlyCollection<ReporteMascota>> ObtenerCandidatosAsync(
        Especie especie,
        string ciudad,
        DateTime fechaDesdeUtc,
        CancellationToken cancellationToken = default);

    /// <summary>Obtiene los reportes creados por un usuario.</summary>
    Task<IReadOnlyCollection<ReporteMascota>> ObtenerPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);

    /// <summary>Inserta o actualiza un reporte.</summary>
    Task GuardarAsync(ReporteMascota reporte, CancellationToken cancellationToken = default);
}
