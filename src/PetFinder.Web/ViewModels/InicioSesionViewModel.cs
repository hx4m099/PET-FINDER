using System.ComponentModel.DataAnnotations;

namespace PetFinder.Web.ViewModels;

/// <summary>Campos del formulario web de inicio de sesión.</summary>
public sealed class InicioSesionViewModel
{
    public string? ReturnUrl { get; set; }

    [Required, EmailAddress, StringLength(254)]
    [Display(Name = "Correo electrónico")]
    public string CorreoElectronico { get; set; } = string.Empty;

    [Required, StringLength(128)]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasena { get; set; } = string.Empty;
}
