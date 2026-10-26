namespace PetFinder.Web.ViewModels;

/// <summary>Opciones de presentación del mapa público definidas en appsettings.</summary>
public sealed class MapaOptions
{
    public const string SectionName = "MapaOptions";

    public decimal CentroLatitud { get; set; } = 10.4630m;
    public decimal CentroLongitud { get; set; } = -73.2530m;
    public int ZoomInicial { get; set; } = 13;
}
