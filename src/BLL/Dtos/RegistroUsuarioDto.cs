using System.ComponentModel.DataAnnotations;
using PetFinder.Entity.Constants;

namespace PetFinder.BLL.Dtos;

/// <summary>Datos necesarios para registrar una cuenta de adoptante.</summary>
public sealed class RegistroUsuarioDto
{
    [Required, StringLength(LimitesEntidad.NombreUsuario, MinimumLength = 2)]
    public string NombreCompleto { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(LimitesEntidad.CorreoElectronico)]
    public string CorreoElectronico { get; init; } = string.Empty;

    [Required, MinLength(8), StringLength(128)]
    public string Contrasena { get; init; } = string.Empty;
}
