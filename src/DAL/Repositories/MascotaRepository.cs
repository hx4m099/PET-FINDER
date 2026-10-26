using PetFinder.DAL.Interfaces;
using PetFinder.Entity.Entities;
using PetFinder.Entity.Enums;

namespace PetFinder.DAL.Repositories;

/// <summary>Repositorio en memoria de mascotas publicadas por refugios.</summary>
public sealed class MascotaRepository(InMemoryDataStore store) : IMascotaRepository
{
    /// <inheritdoc />
    public Task<Mascota?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        store.Mascotas.TryGetValue(id, out var mascota);
        return Task.FromResult(mascota);
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<Mascota>> ObtenerDisponiblesAdopcionAsync(
        Guid? refugioId = null,
        Especie? especie = null,
        int? edadMinimaMeses = null,
        int? edadMaximaMeses = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var consulta = store.Mascotas.Values.Where(mascota => mascota.Activa && mascota.DisponibleAdopcion);
        if (refugioId.HasValue)
        {
            consulta = consulta.Where(mascota => mascota.RefugioId == refugioId.Value);
        }

        if (especie.HasValue)
        {
            consulta = consulta.Where(mascota => mascota.Especie == especie.Value);
        }

        if (edadMinimaMeses.HasValue)
        {
            consulta = consulta.Where(mascota => mascota.EdadMeses.HasValue && mascota.EdadMeses.Value >= edadMinimaMeses.Value);
        }

        if (edadMaximaMeses.HasValue)
        {
            consulta = consulta.Where(mascota => mascota.EdadMeses.HasValue && mascota.EdadMeses.Value <= edadMaximaMeses.Value);
        }

        IReadOnlyCollection<Mascota> resultado = consulta.OrderBy(mascota => mascota.Nombre).ToArray();
        return Task.FromResult(resultado);
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<Mascota>> ObtenerTodasAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyCollection<Mascota> mascotas = store.Mascotas.Values.OrderBy(mascota => mascota.Nombre).ToArray();
        return Task.FromResult(mascotas);
    }

    /// <inheritdoc />
    public Task GuardarAsync(Mascota mascota, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mascota);
        cancellationToken.ThrowIfCancellationRequested();
        store.Mascotas.AddOrUpdate(mascota.Id, mascota, (_, _) => mascota);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> EliminarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(store.Mascotas.TryRemove(id, out _));
    }
}
