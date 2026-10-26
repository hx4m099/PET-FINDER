using Microsoft.Extensions.Logging;
using PetFinder.BLL.Dtos;
using PetFinder.BLL.Exceptions;
using PetFinder.DAL.Interfaces;
using PetFinder.Entity.Entities;
using PetFinder.Entity.Enums;

namespace PetFinder.BLL.Services;

/// <summary>Aplica permisos del refugio sobre su perfil y sus publicaciones de adopción.</summary>
public sealed class GestionRefugioService(
    IUsuarioRepository usuarioRepository,
    IRefugioRepository refugioRepository,
    IMascotaRepository mascotaRepository,
    ILogger<GestionRefugioService> logger)
{
    /// <summary>Obtiene el perfil aliado y las mascotas pertenecientes al refugio del usuario.</summary>
    public async Task<(Refugio Refugio, IReadOnlyCollection<Mascota> Mascotas)> ObtenerPanelAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        var refugio = await ObtenerRefugioAutorizadoAsync(usuarioId, cancellationToken);
        var mascotas = (await mascotaRepository.ObtenerTodasAsync(cancellationToken))
            .Where(mascota => mascota.RefugioId == refugio.Id)
            .OrderBy(mascota => mascota.Nombre)
            .ToArray();
        return (refugio, mascotas);
    }

    /// <summary>Publica o edita una mascota exclusivamente dentro del refugio asociado al usuario.</summary>
    public async Task<Mascota> GuardarMascotaAsync(
        Guid usuarioId,
        GuardarMascotaRefugioDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var refugio = await ObtenerRefugioAutorizadoAsync(usuarioId, cancellationToken);
        if (string.IsNullOrWhiteSpace(dto.Nombre) || dto.Nombre.Trim().Length > 120
            || string.IsNullOrWhiteSpace(dto.Ciudad) || dto.Ciudad.Trim().Length > 100
            || string.IsNullOrWhiteSpace(dto.Descripcion) || dto.Descripcion.Trim().Length > 2000
            || string.IsNullOrWhiteSpace(dto.FotoUrl) || dto.FotoUrl.Trim().Length > 2048
            || dto.EdadMeses is < 0 or > 600
            || !Enum.IsDefined(dto.Especie))
        {
            throw new BusinessRuleException("Los datos de la mascota no son válidos.");
        }

        Mascota mascota;
        if (dto.Id.HasValue)
        {
            mascota = await mascotaRepository.ObtenerPorIdAsync(dto.Id.Value, cancellationToken)
                ?? throw new EntityNotFoundException("mascota", dto.Id.Value);
            if (mascota.RefugioId != refugio.Id)
            {
                throw new BusinessRuleException("No puedes editar publicaciones de otro refugio.");
            }
        }
        else
        {
            mascota = new Mascota { RefugioId = refugio.Id };
        }

        mascota.Nombre = dto.Nombre.Trim();
        mascota.Especie = dto.Especie;
        mascota.Raza = dto.Raza.Trim();
        mascota.EdadMeses = dto.EdadMeses;
        mascota.Descripcion = dto.Descripcion.Trim();
        mascota.FotoUrl = dto.FotoUrl.Trim();
        mascota.Ciudad = dto.Ciudad.Trim();
        mascota.DisponibleAdopcion = dto.DisponibleAdopcion;
        mascota.Activa = true;
        mascota.FechaPublicacionUtc = DateTime.UtcNow;
        await mascotaRepository.GuardarAsync(mascota, cancellationToken);
        logger.LogInformation("El usuario del refugio {UsuarioId} guardó la mascota {MascotaId}.", usuarioId, mascota.Id);
        return mascota;
    }

    private async Task<Refugio> ObtenerRefugioAutorizadoAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken);
        if (usuario is null || !usuario.Activo)
        {
            throw new EntityNotFoundException("usuario", usuarioId);
        }

        if (usuario.Rol != Rol.Refugio)
        {
            throw new BusinessRuleException("Esta operación requiere una cuenta de refugio.");
        }

        return (await refugioRepository.ObtenerTodosAsync(cancellationToken: cancellationToken))
            .FirstOrDefault(refugio => refugio.UsuarioId == usuarioId && refugio.Activo)
            ?? throw new EntityNotFoundException("refugio activo", usuarioId);
    }
}
