using System.ComponentModel.DataAnnotations;
using PetFinder.Entity.Enums;

namespace PetFinder.Web.ViewModels;

/// <summary>Formulario MVC para publicar una mascota perdida o encontrada.</summary>
public sealed class ReporteViewModel
{
    [Display(Name = "Tipo de reporte")]
    public TipoReporte Tipo { get; set; }

    [Display(Name = "Especie")]
    public Especie Especie { get; set; }

    [StringLength(120)]
    [Display(Name = "Nombre de la mascota (si se conoce)")]
    public string NombreMascota { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Raza")]
    public string Raza { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 2)]
    [Display(Name = "Ciudad")]
    public string Ciudad { get; set; } = string.Empty;

    [Required, StringLength(2000, MinimumLength = 5)]
    [Display(Name = "Descripción")]
    public string Descripcion { get; set; } = string.Empty;

    [StringLength(2048)]
    [Display(Name = "URL de foto")]
    public string FotoUrl { get; set; } = string.Empty;

    [Display(Name = "Subir una foto (JPG, PNG o WebP)")]
    public IFormFile? Foto { get; set; }

    [Required, DataType(DataType.DateTime)]
    [Display(Name = "Fecha y hora del evento")]
    public DateTime FechaEvento { get; set; } = DateTime.Now;
}
