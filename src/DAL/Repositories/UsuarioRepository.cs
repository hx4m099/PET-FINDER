using PetFinder.DAL.Interfaces;
using PetFinder.Entity.Entities;

namespace PetFinder.DAL.Repositories;

/// <summary>Repositorio en memoria de usuarios.</summary>
public sealed class UsuarioRepository(InMemoryDataStore store) : IUsuarioRepository
{
    /// <inheritdoc />
    public Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        store.Usuarios.TryGetValue(id, out var usuario);
        return Task.FromResult(usuario);
    }

    /// <inheritdoc />
    public Task<Usuario?> ObtenerPorCorreoAsync(string correoElectronico, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(correoElectronico);
        cancellationToken.ThrowIfCancellationRequested();
        var usuario = store.Usuarios.Values.FirstOrDefault(
            item => string.Equals(item.CorreoElectronico, correoElectronico, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(usuario);
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<Usuario>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyCollection<Usuario> usuarios = store.Usuarios.Values.OrderBy(item => item.NombreCompleto).ToArray();
        return Task.FromResult(usuarios);
    }

    /// <inheritdoc />
    public Task GuardarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(usuario);
        cancellationToken.ThrowIfCancellationRequested();
        store.Usuarios.AddOrUpdate(usuario.Id, usuario, (_, _) => usuario);
        return Task.CompletedTask;
    }
}
