using System.ComponentModel.DataAnnotations;
using PetFinder.Entity.Enums;

namespace PetFinder.Web.ViewModels;

/// <summary>Campos editables de una mascota publicada por un refugio aliado.</summary>
public sealed class MascotaRefugioViewModel
{
    public Guid? Id { get; set; }

    [Required, StringLength(120, MinimumLength = 1)]
    public string Nombre { get; set; } = string.Empty;

    public Especie Especie { get; set; }

    [StringLength(100)]
    public string Raza { get; set; } = string.Empty;

    [Range(0, 600)]
    public int? EdadMeses { get; set; }

    [Required, StringLength(2000, MinimumLength = 5)]
    public string Descripcion { get; set; } = string.Empty;

    [Required, StringLength(2048)]
    [Display(Name = "URL de foto")]
    public string FotoUrl { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Ciudad { get; set; } = string.Empty;

    public bool DisponibleAdopcion { get; set; } = true;
}
