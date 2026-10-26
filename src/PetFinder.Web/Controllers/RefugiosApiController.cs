using Microsoft.AspNetCore.Mvc;
using PetFinder.BLL.Services;

namespace PetFinder.Web.Controllers;

/// <summary>API pública de ubicaciones verificadas para la experiencia del mapa.</summary>
[ApiController]
[Route("api/refugios")]
public sealed class RefugiosApiController(IRefugioService refugioService) : ControllerBase
{
    /// <summary>Entrega refugios activos con coordenadas válidas; admite orden por cercanía.</summary>
    [HttpGet]
    [Produces("application/json")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
    public async Task<IActionResult> Obtener(
        [FromQuery] decimal? latitud,
        [FromQuery] decimal? longitud,
        CancellationToken cancellationToken)
    {
        var refugios = await refugioService.ObtenerMapaAsync(latitud, longitud, cancellationToken);
        return Ok(refugios);
    }
}
