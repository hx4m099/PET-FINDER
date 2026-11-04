using System.ComponentModel.DataAnnotations;
using PetFinder.Entity.Enums;
using PetFinder.Entity.Constants;

namespace PetFinder.BLL.Dtos;

/// <summary>Parámetros de búsqueda previa de reportes compatibles.</summary>
public sealed class BuscarCoincidenciasDto
{
    public Especie Especie { get; init; }

    [Required, StringLength(LimitesEntidad.Ciudad, MinimumLength = 2)]
    public string Ciudad { get; init; } = string.Empty;

    public Guid? ExcluirReporteId { get; init; }
    public TipoReporte? TipoReporteOrigen { get; init; }
    public int DiasRecientes { get; init; } = 30;
}
