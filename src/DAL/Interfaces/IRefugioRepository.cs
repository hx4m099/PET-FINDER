using PetFinder.Entity.Entities;

namespace PetFinder.DAL.Interfaces;

/// <summary>Abstracción de persistencia para refugios.</summary>
public interface IRefugioRepository
{
    /// <summary>Busca un refugio por su identificador.</summary>
    Task<Refugio?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Obtiene los refugios y permite excluir los inactivos.</summary>
    Task<IReadOnlyCollection<Refugio>> ObtenerTodosAsync(bool soloActivos = false, CancellationToken cancellationToken = default);

    /// <summary>Inserta o actualiza un refugio.</summary>
    Task GuardarAsync(Refugio refugio, CancellationToken cancellationToken = default);
}
