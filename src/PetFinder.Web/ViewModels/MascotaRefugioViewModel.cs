using System.ComponentModel.DataAnnotations;
using PetFinder.Entity.Enums;
using PetFinder.Entity.Constants;

namespace PetFinder.Web.ViewModels;

/// <summary>Campos editables de una mascota publicada por un refugio aliado.</summary>
public sealed class MascotaRefugioViewModel
{
    public Guid? Id { get; set; }

    [Required, StringLength(LimitesEntidad.NombreMascota, MinimumLength = 1)]
    public string Nombre { get; set; } = string.Empty;

    public Especie Especie { get; set; }

    [StringLength(LimitesEntidad.Raza)]
    public string Raza { get; set; } = string.Empty;

    [Range(0, 600)]
    public int? EdadMeses { get; set; }

    [Required, StringLength(LimitesEntidad.Descripcion, MinimumLength = 5)]
    public string Descripcion { get; set; } = string.Empty;

    [Required, StringLength(LimitesEntidad.Url)]
    [Display(Name = "URL de foto")]
    public string FotoUrl { get; set; } = string.Empty;

    [Required, StringLength(LimitesEntidad.Ciudad)]
    public string Ciudad { get; set; } = string.Empty;

    public bool DisponibleAdopcion { get; set; } = true;
}
