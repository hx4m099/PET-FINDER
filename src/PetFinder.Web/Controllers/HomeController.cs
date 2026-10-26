using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PetFinder.BLL.Services;
using PetFinder.Web.ViewModels;

namespace PetFinder.Web.Controllers;

/// <summary>Acciones públicas de inicio de PetFinder.</summary>
public sealed class HomeController(
    AdopcionService adopcionService,
    IRefugioService refugioService,
    IEstadisticasService estadisticasService,
    ReporteService reporteService,
    IOptions<MapaOptions> mapaOptions) : Controller
{
    /// <summary>Presenta la landing pública con datos agregados exclusivamente por BLL.</summary>
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var estadisticas = await estadisticasService.ObtenerAsync(cancellationToken);
        var refugios = await refugioService.ObtenerMapaAsync(cancellationToken: cancellationToken);
        var mascotas = await adopcionService.ObtenerCatalogoAsync(cancellationToken: cancellationToken);
        var avisos = await reporteService.ObtenerTableroAsync(6, cancellationToken);
        var opciones = mapaOptions.Value;

        return View(new HomeViewModel
        {
            Estadisticas = estadisticas,
            Refugios = refugios,
            MascotasDestacadas = mascotas.Take(4).ToArray(),
            AvisosRecientes = avisos,
            MapaCentroLatitud = opciones.CentroLatitud,
            MapaCentroLongitud = opciones.CentroLongitud,
            MapaZoomInicial = opciones.ZoomInicial
        });
    }
}
