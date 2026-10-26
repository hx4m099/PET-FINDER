using System.ComponentModel.DataAnnotations;

namespace PetFinder.BLL.Dtos;

/// <summary>Datos necesarios para registrar una cuenta de adoptante.</summary>
public sealed class RegistroUsuarioDto
{
    [Required, StringLength(120, MinimumLength = 2)]
    public string NombreCompleto { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string CorreoElectronico { get; init; } = string.Empty;

    [Required, MinLength(8), StringLength(128)]
    public string Contrasena { get; init; } = string.Empty;
}
