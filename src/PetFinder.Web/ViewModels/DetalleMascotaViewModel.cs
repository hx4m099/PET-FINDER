using System.ComponentModel.DataAnnotations;
using PetFinder.Entity.Entities;

namespace PetFinder.Web.ViewModels;

/// <summary>Detalle visible de una mascota y datos de contacto del refugio.</summary>
public sealed class DetalleMascotaViewModel
{
    public Mascota Mascota { get; set; } = new();
    public Refugio Refugio { get; set; } = new();

    [Required, DataType(DataType.DateTime)]
    [Display(Name = "Fecha y hora (tu hora local)")]
    public DateTime FechaInicioLocal { get; set; } = DateTime.Now.AddDays(1);

    [Range(15, 120)]
    [Display(Name = "Duración (minutos)")]
    public int DuracionMinutos { get; set; } = 30;

    [StringLength(1000)]
    [Display(Name = "Mensaje para el refugio")]
    public string Notas { get; set; } = string.Empty;
}
