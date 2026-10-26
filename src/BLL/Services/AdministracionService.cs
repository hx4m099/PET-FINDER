using Microsoft.Extensions.Logging;
using PetFinder.BLL.Exceptions;
using PetFinder.DAL.Interfaces;
using PetFinder.Entity.Entities;
using PetFinder.Entity.Enums;

namespace PetFinder.BLL.Services;

/// <summary>Aplica permisos administrativos y operaciones básicas de moderación.</summary>
public sealed class AdministracionService(
    IUsuarioRepository usuarioRepository,
    IRefugioRepository refugioRepository,
    ILogger<AdministracionService> logger)
{
    /// <summary>Obtiene usuarios y refugios para el panel, solo para administradores activos.</summary>
    public async Task<(IReadOnlyCollection<Usuario> Usuarios, IReadOnlyCollection<Refugio> Refugios)> ObtenerResumenAsync(
        Guid administradorId,
        CancellationToken cancellationToken = default)
    {
        await ValidarAdministradorAsync(administradorId, cancellationToken);
        var usuarios = await usuarioRepository.ObtenerTodosAsync(cancellationToken);
        var refugios = await refugioRepository.ObtenerTodosAsync(cancellationToken: cancellationToken);
        return (usuarios, refugios);
    }

    /// <summary>Activa o suspende una cuenta; un administrador no puede suspenderse a sí mismo.</summary>
    public async Task<Usuario> CambiarEstadoUsuarioAsync(
        Guid administradorId,
        Guid usuarioId,
        bool activo,
        CancellationToken cancellationToken = default)
    {
        await ValidarAdministradorAsync(administradorId, cancellationToken);
        if (administradorId == usuarioId)
        {
            throw new BusinessRuleException("No puedes cambiar el estado de tu propia cuenta administrativa.");
        }

        var usuario = await usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken)
            ?? throw new EntityNotFoundException("usuario", usuarioId);
        usuario.Activo = activo;
        await usuarioRepository.GuardarAsync(usuario, cancellationToken);
        logger.LogInformation("El administrador {AdministradorId} cambió el estado de {UsuarioId} a {Activo}.", administradorId, usuarioId, activo);
        return usuario;
    }

    /// <summary>Activa o suspende un refugio aliado.</summary>
    public async Task<Refugio> CambiarEstadoRefugioAsync(
        Guid administradorId,
        Guid refugioId,
        bool activo,
        CancellationToken cancellationToken = default)
    {
        await ValidarAdministradorAsync(administradorId, cancellationToken);
        var refugio = await refugioRepository.ObtenerPorIdAsync(refugioId, cancellationToken)
            ?? throw new EntityNotFoundException("refugio", refugioId);
        refugio.Activo = activo;
        await refugioRepository.GuardarAsync(refugio, cancellationToken);
        logger.LogInformation("El administrador {AdministradorId} cambió el estado del refugio {RefugioId} a {Activo}.", administradorId, refugioId, activo);
        return refugio;
    }

    private async Task ValidarAdministradorAsync(Guid administradorId, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(administradorId, cancellationToken);
        if (usuario is null || !usuario.Activo)
        {
            throw new EntityNotFoundException("usuario", administradorId);
        }

        if (usuario.Rol != Rol.Administrador)
        {
            throw new BusinessRuleException("Esta operación requiere una cuenta de administrador.");
        }
    }
}
