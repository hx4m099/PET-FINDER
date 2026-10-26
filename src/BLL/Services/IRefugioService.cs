using PetFinder.BLL.Dtos;

namespace PetFinder.BLL.Services;

/// <summary>Consulta refugios activos con ubicación válida para el catálogo y el mapa.</summary>
public interface IRefugioService
{
    /// <summary>Obtiene refugios geolocalizados y opcionalmente los ordena por distancia al origen.</summary>
    Task<IReadOnlyCollection<RefugioMapaDto>> ObtenerMapaAsync(
        decimal? latitudOrigen = null,
        decimal? longitudOrigen = null,
        CancellationToken cancellationToken = default);
}
