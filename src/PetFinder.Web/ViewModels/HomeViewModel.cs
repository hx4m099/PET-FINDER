using PetFinder.BLL.Dtos;
using PetFinder.Entity.Entities;

namespace PetFinder.Web.ViewModels;

/// <summary>Datos de presentación ensamblados por HomeController para la landing.</summary>
public sealed class HomeViewModel
{
    public EstadisticasImpactoDto Estadisticas { get; init; } = new(0, 0, 0, 0);
    public IReadOnlyCollection<RefugioMapaDto> Refugios { get; init; } = [];
    public IReadOnlyCollection<Mascota> MascotasDestacadas { get; init; } = [];
    public IReadOnlyCollection<ReporteMascota> AvisosRecientes { get; init; } = [];
    public decimal MapaCentroLatitud { get; init; }
    public decimal MapaCentroLongitud { get; init; }
    public int MapaZoomInicial { get; init; }
}
