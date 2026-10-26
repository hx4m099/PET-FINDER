using System.ComponentModel.DataAnnotations;
using PetFinder.Entity.Enums;

namespace PetFinder.BLL.Dtos;

/// <summary>Campos editables para publicar un reporte de mascota.</summary>
public sealed class CrearReporteMascotaDto
{
    public TipoReporte Tipo { get; init; }
    public Especie Especie { get; init; }

    [StringLength(120)]
    public string NombreMascota { get; init; } = string.Empty;

    [StringLength(100)]
    public string Raza { get; init; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 2)]
    public string Ciudad { get; init; } = string.Empty;

    [Required, StringLength(2000, MinimumLength = 5)]
    public string Descripcion { get; init; } = string.Empty;

    [Required, StringLength(2048)]
    public string FotoUrl { get; init; } = string.Empty;

    public DateTime FechaEventoUtc { get; init; }
}
