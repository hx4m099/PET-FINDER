using PetFinder.Entity.Entities;
using PetFinder.Entity.Enums;

namespace PetFinder.DAL.Interfaces;

/// <summary>Abstracción de persistencia para mascotas.</summary>
public interface IMascotaRepository
{
    /// <summary>Busca una mascota por su identificador.</summary>
    Task<Mascota?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Obtiene mascotas disponibles aplicando filtros opcionales de catálogo.</summary>
    Task<IReadOnlyCollection<Mascota>> ObtenerDisponiblesAdopcionAsync(
        Guid? refugioId = null,
        Especie? especie = null,
        int? edadMinimaMeses = null,
        int? edadMaximaMeses = null,
        CancellationToken cancellationToken = default);

    /// <summary>Obtiene todas las mascotas publicadas.</summary>
    Task<IReadOnlyCollection<Mascota>> ObtenerTodasAsync(CancellationToken cancellationToken = default);

    /// <summary>Inserta o actualiza una mascota.</summary>
    Task GuardarAsync(Mascota mascota, CancellationToken cancellationToken = default);

    /// <summary>Elimina una mascota y devuelve si existía.</summary>
    Task<bool> EliminarAsync(Guid id, CancellationToken cancellationToken = default);
}
