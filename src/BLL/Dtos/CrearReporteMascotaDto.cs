using System.ComponentModel.DataAnnotations;
using PetFinder.Entity.Enums;
using PetFinder.Entity.Constants;

namespace PetFinder.BLL.Dtos;

/// <summary>Campos editables para publicar un reporte de mascota.</summary>
public sealed class CrearReporteMascotaDto
{
    public TipoReporte Tipo { get; init; }
    public Especie Especie { get; init; }

    [StringLength(LimitesEntidad.NombreMascota)]
    public string NombreMascota { get; init; } = string.Empty;

    [StringLength(LimitesEntidad.Raza)]
    public string Raza { get; init; } = string.Empty;

    [Required, StringLength(LimitesEntidad.Ciudad, MinimumLength = 2)]
    public string Ciudad { get; init; } = string.Empty;

    [Required, StringLength(LimitesEntidad.Descripcion, MinimumLength = 5)]
    public string Descripcion { get; init; } = string.Empty;

    [Required, StringLength(LimitesEntidad.Url)]
    public string FotoUrl { get; init; } = string.Empty;

    public DateTime FechaEventoUtc { get; init; }
}
