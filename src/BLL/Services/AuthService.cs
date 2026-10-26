using FluentValidation;
using Microsoft.Extensions.Logging;
using PetFinder.BLL.Dtos;
using PetFinder.BLL.Exceptions;
using PetFinder.DAL.Interfaces;
using PetFinder.Entity.Entities;
using PetFinder.Entity.Enums;

namespace PetFinder.BLL.Services;

/// <summary>Gestiona el registro y la autenticación simulada de usuarios.</summary>
public sealed class AuthService(
    IUsuarioRepository usuarioRepository,
    IValidator<RegistroUsuarioDto> registroValidator,
    IValidator<InicioSesionDto> inicioSesionValidator,
    ILogger<AuthService> logger)
{
    /// <summary>Registra una cuenta de adoptante usando hash con sal; nunca almacena la contraseña en claro.</summary>
    public async Task<SesionUsuarioDto> RegistrarAsync(RegistroUsuarioDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        await registroValidator.ValidateAndThrowAsync(dto, cancellationToken);
        var correo = dto.CorreoElectronico.Trim();
        if (await usuarioRepository.ObtenerPorCorreoAsync(correo, cancellationToken) is not null)
        {
            throw new BusinessRuleException("Ya existe una cuenta registrada con ese correo electrónico.");
        }

        var usuario = new Usuario
        {
            NombreCompleto = dto.NombreCompleto.Trim(),
            CorreoElectronico = correo,
            PasswordHash = PasswordHashService.CrearHash(dto.Contrasena),
            Rol = Rol.Adoptante,
            Activo = true
        };
        await usuarioRepository.GuardarAsync(usuario, cancellationToken);
        logger.LogInformation("Se registró la cuenta {UsuarioId} con rol {Rol}.", usuario.Id, usuario.Rol);
        return Mapear(usuario);
    }

    /// <summary>Autentica correo y contraseña y devuelve solo la identidad no sensible.</summary>
    public async Task<SesionUsuarioDto> IniciarSesionAsync(InicioSesionDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        await inicioSesionValidator.ValidateAndThrowAsync(dto, cancellationToken);
        var usuario = await usuarioRepository.ObtenerPorCorreoAsync(dto.CorreoElectronico.Trim(), cancellationToken);
        if (usuario is null || !usuario.Activo || !PasswordHashService.Verificar(dto.Contrasena, usuario.PasswordHash))
        {
            logger.LogWarning("Falló una autenticación de PetFinder.");
            throw new BusinessRuleException("El correo electrónico o la contraseña no son válidos.");
        }

        logger.LogInformation("Se autenticó el usuario {UsuarioId} con rol {Rol}.", usuario.Id, usuario.Rol);
        return Mapear(usuario);
    }

    private static SesionUsuarioDto Mapear(Usuario usuario) =>
        new(usuario.Id, usuario.NombreCompleto, usuario.CorreoElectronico, usuario.Rol);
}
