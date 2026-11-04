using System.ComponentModel.DataAnnotations;
using PetFinder.Entity.Constants;

namespace PetFinder.BLL.Dtos;

/// <summary>Credenciales entregadas para iniciar sesión.</summary>
public sealed class InicioSesionDto
{
    [Required, EmailAddress, StringLength(LimitesEntidad.CorreoElectronico)]
    public string CorreoElectronico { get; init; } = string.Empty;

    [Required, StringLength(128)]
    public string Contrasena { get; init; } = string.Empty;
}
