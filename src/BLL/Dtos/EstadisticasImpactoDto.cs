namespace PetFinder.BLL.Dtos;

/// <summary>Indicadores agregados desde los estados explícitos del dominio.</summary>
public sealed record EstadisticasImpactoDto(
    int MascotasReunidas,
    int RefugiosAliados,
    int Adopciones,
    int ReportesActivos);
