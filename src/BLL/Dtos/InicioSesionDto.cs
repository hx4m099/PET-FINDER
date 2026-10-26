using System.ComponentModel.DataAnnotations;

namespace PetFinder.BLL.Dtos;

/// <summary>Credenciales entregadas para iniciar sesión.</summary>
public sealed class InicioSesionDto
{
    [Required, EmailAddress, StringLength(254)]
    public string CorreoElectronico { get; init; } = string.Empty;

    [Required, StringLength(128)]
    public string Contrasena { get; init; } = string.Empty;
}
