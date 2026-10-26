using PetFinder.BLL.Dtos;

namespace PetFinder.BLL.Services;

/// <summary>Proporciona métricas agregadas y seguras para mostrar en la landing.</summary>
public interface IEstadisticasService
{
    /// <summary>Calcula las cifras a partir de registros de dominio activos e históricos.</summary>
    Task<EstadisticasImpactoDto> ObtenerAsync(CancellationToken cancellationToken = default);
}
