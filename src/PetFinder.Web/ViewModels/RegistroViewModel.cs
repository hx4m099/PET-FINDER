using System.ComponentModel.DataAnnotations;

namespace PetFinder.Web.ViewModels;

/// <summary>Campos del formulario web para crear una cuenta.</summary>
public sealed class RegistroViewModel
{
    [Required, StringLength(120, MinimumLength = 2, ErrorMessage = "Ingresa tu nombre completo.")]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    [Display(Name = "Correo electrónico")]
    public string CorreoElectronico { get; set; } = string.Empty;

    [Required, MinLength(8), StringLength(128)]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasena { get; set; } = string.Empty;

    [Required, Compare(nameof(Contrasena), ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmarContrasena { get; set; } = string.Empty;
}
