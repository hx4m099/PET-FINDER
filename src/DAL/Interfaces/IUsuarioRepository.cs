using PetFinder.Entity.Entities;

namespace PetFinder.DAL.Interfaces;

/// <summary>Abstracción de persistencia para usuarios.</summary>
public interface IUsuarioRepository
{
    /// <summary>Busca un usuario por su identificador.</summary>
    Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Busca un usuario por correo electrónico, sin distinguir mayúsculas.</summary>
    Task<Usuario?> ObtenerPorCorreoAsync(string correoElectronico, CancellationToken cancellationToken = default);

    /// <summary>Obtiene todos los usuarios registrados.</summary>
    Task<IReadOnlyCollection<Usuario>> ObtenerTodosAsync(CancellationToken cancellationToken = default);

    /// <summary>Inserta o actualiza un usuario.</summary>
    Task GuardarAsync(Usuario usuario, CancellationToken cancellationToken = default);
}
