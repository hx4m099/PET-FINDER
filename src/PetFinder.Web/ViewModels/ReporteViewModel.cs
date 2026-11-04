using System.ComponentModel.DataAnnotations;
using PetFinder.Entity.Enums;
using PetFinder.Entity.Constants;

namespace PetFinder.Web.ViewModels;

/// <summary>Formulario MVC para publicar una mascota perdida o encontrada.</summary>
public sealed class ReporteViewModel
{
    [Display(Name = "Tipo de reporte")]
    public TipoReporte Tipo { get; set; }

    [Display(Name = "Especie")]
    public Especie Especie { get; set; }

    [StringLength(LimitesEntidad.NombreMascota)]
    [Display(Name = "Nombre de la mascota (si se conoce)")]
    public string NombreMascota { get; set; } = string.Empty;

    [StringLength(LimitesEntidad.Raza)]
    [Display(Name = "Raza")]
    public string Raza { get; set; } = string.Empty;

    [Required, StringLength(LimitesEntidad.Ciudad, MinimumLength = 2)]
    [Display(Name = "Ciudad")]
    public string Ciudad { get; set; } = string.Empty;

    [Required, StringLength(LimitesEntidad.Descripcion, MinimumLength = 5)]
    [Display(Name = "Descripción")]
    public string Descripcion { get; set; } = string.Empty;

    [StringLength(LimitesEntidad.Url)]
    [Display(Name = "URL de foto")]
    public string FotoUrl { get; set; } = string.Empty;

    [Display(Name = "Subir una foto (JPG, PNG o WebP)")]
    public IFormFile? Foto { get; set; }

    [Required, DataType(DataType.DateTime)]
    [Display(Name = "Fecha y hora del evento")]
    public DateTime FechaEvento { get; set; } = DateTime.Now;
}
