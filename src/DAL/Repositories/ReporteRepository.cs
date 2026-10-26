using PetFinder.DAL.Interfaces;
using PetFinder.Entity.Entities;
using PetFinder.Entity.Enums;

namespace PetFinder.DAL.Repositories;

/// <summary>Repositorio en memoria de reportes de mascotas perdidas o encontradas.</summary>
public sealed class ReporteRepository(InMemoryDataStore store) : IReporteRepository
{
    /// <inheritdoc />
    public Task<ReporteMascota?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        store.Reportes.TryGetValue(id, out var reporte);
        return Task.FromResult(reporte);
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<ReporteMascota>> ObtenerCandidatosAsync(
        Especie especie,
        string ciudad,
        DateTime fechaDesdeUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ciudad);
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyCollection<ReporteMascota> candidatos = store.Reportes.Values
            .Where(reporte => reporte.Activo
                && reporte.Especie == especie
                && string.Equals(reporte.Ciudad, ciudad, StringComparison.OrdinalIgnoreCase)
                && reporte.FechaReporteUtc >= fechaDesdeUtc)
            .OrderByDescending(reporte => reporte.FechaReporteUtc)
            .ToArray();
        return Task.FromResult(candidatos);
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<ReporteMascota>> ObtenerPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyCollection<ReporteMascota> reportes = store.Reportes.Values
            .Where(reporte => reporte.UsuarioId == usuarioId)
            .OrderByDescending(reporte => reporte.FechaReporteUtc)
            .ToArray();
        return Task.FromResult(reportes);
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<ReporteMascota>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyCollection<ReporteMascota> reportes = store.Reportes.Values
            .OrderByDescending(reporte => reporte.FechaReporteUtc)
            .ToArray();
        return Task.FromResult(reportes);
    }

    /// <inheritdoc />
    public Task GuardarAsync(ReporteMascota reporte, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reporte);
        cancellationToken.ThrowIfCancellationRequested();
        store.Reportes.AddOrUpdate(reporte.Id, reporte, (_, _) => reporte);
        return Task.CompletedTask;
    }
}
