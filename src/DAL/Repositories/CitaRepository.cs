using PetFinder.DAL.Interfaces;
using PetFinder.Entity.Entities;
using PetFinder.Entity.Enums;

namespace PetFinder.DAL.Repositories;

/// <summary>Repositorio en memoria de citas de adopción.</summary>
public sealed class CitaRepository(InMemoryDataStore store) : ICitaRepository
{
    /// <inheritdoc />
    public Task<Cita?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        store.Citas.TryGetValue(id, out var cita);
        return Task.FromResult(cita);
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<Cita>> ObtenerPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyCollection<Cita> citas = store.Citas.Values
            .Where(cita => cita.UsuarioId == usuarioId)
            .OrderBy(cita => cita.FechaInicioUtc)
            .ToArray();
        return Task.FromResult(citas);
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<Cita>> ObtenerPorRefugioYFechaAsync(
        Guid refugioId,
        DateTime inicioUtc,
        DateTime finUtc,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyCollection<Cita> citas = store.Citas.Values
            .Where(cita => cita.RefugioId == refugioId
                && cita.FechaInicioUtc >= inicioUtc
                && cita.FechaInicioUtc < finUtc)
            .OrderBy(cita => cita.FechaInicioUtc)
            .ToArray();
        return Task.FromResult(citas);
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<Cita>> ObtenerCrucesDeHorarioAsync(
        Guid refugioId,
        DateTime inicioUtc,
        DateTime finUtc,
        Guid? excluirCitaId = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyCollection<Cita> cruces = store.Citas.Values
            .Where(cita => cita.RefugioId == refugioId
                && cita.Id != excluirCitaId
                && cita.Estado is EstadoCita.Pendiente or EstadoCita.Confirmada
                && cita.FechaInicioUtc < finUtc
                && inicioUtc < cita.FechaFinUtc)
            .OrderBy(cita => cita.FechaInicioUtc)
            .ToArray();
        return Task.FromResult(cruces);
    }

    /// <inheritdoc />
    public Task GuardarAsync(Cita cita, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cita);
        cancellationToken.ThrowIfCancellationRequested();
        store.Citas.AddOrUpdate(cita.Id, cita, (_, _) => cita);
        return Task.CompletedTask;
    }
}
