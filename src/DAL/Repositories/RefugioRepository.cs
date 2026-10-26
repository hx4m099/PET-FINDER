using PetFinder.DAL.Interfaces;
using PetFinder.Entity.Entities;

namespace PetFinder.DAL.Repositories;

/// <summary>Repositorio en memoria de refugios aliados.</summary>
public sealed class RefugioRepository(InMemoryDataStore store) : IRefugioRepository
{
    /// <inheritdoc />
    public Task<Refugio?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        store.Refugios.TryGetValue(id, out var refugio);
        return Task.FromResult(refugio);
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<Refugio>> ObtenerTodosAsync(bool soloActivos = false, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var consulta = store.Refugios.Values.AsEnumerable();
        if (soloActivos)
        {
            consulta = consulta.Where(refugio => refugio.Activo);
        }

        IReadOnlyCollection<Refugio> refugios = consulta.OrderBy(refugio => refugio.Nombre).ToArray();
        return Task.FromResult(refugios);
    }

    /// <inheritdoc />
    public Task GuardarAsync(Refugio refugio, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(refugio);
        cancellationToken.ThrowIfCancellationRequested();
        store.Refugios.AddOrUpdate(refugio.Id, refugio, (_, _) => refugio);
        return Task.CompletedTask;
    }
}
