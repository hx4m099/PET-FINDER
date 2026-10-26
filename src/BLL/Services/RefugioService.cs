using PetFinder.BLL.Dtos;
using PetFinder.DAL.Interfaces;
using PetFinder.Entity.Entities;

namespace PetFinder.BLL.Services;

/// <summary>Prepara los refugios aliados para su exposición pública en el mapa.</summary>
public sealed class RefugioService(IRefugioRepository refugioRepository, IMascotaRepository mascotaRepository) : IRefugioService
{
    private const string CiudadMapa = "Valledupar";
    private const double RadioTierraKm = 6371.0088;

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<RefugioMapaDto>> ObtenerMapaAsync(
        decimal? latitudOrigen = null,
        decimal? longitudOrigen = null,
        CancellationToken cancellationToken = default)
    {
        if (latitudOrigen.HasValue != longitudOrigen.HasValue)
        {
            throw new ArgumentException("La latitud y longitud de origen deben enviarse juntas.");
        }

        if (latitudOrigen is < -90m or > 90m)
        {
            throw new ArgumentOutOfRangeException(nameof(latitudOrigen), "La latitud debe estar entre -90 y 90 grados.");
        }

        if (longitudOrigen is < -180m or > 180m)
        {
            throw new ArgumentOutOfRangeException(nameof(longitudOrigen), "La longitud debe estar entre -180 y 180 grados.");
        }

        var refugios = await refugioRepository.ObtenerTodosAsync(soloActivos: true, cancellationToken);
        var mascotasDisponibles = await mascotaRepository.ObtenerDisponiblesAdopcionAsync(cancellationToken: cancellationToken);
        var mascotasPorRefugio = mascotasDisponibles.ToLookup(mascota => mascota.RefugioId);

        var ubicaciones = refugios
            .Where(refugio => refugio.Activo
                && string.Equals(refugio.Ciudad, CiudadMapa, StringComparison.OrdinalIgnoreCase)
                && TieneCoordenadasValidas(refugio))
            .Select(refugio => CrearDto(refugio, mascotasPorRefugio[refugio.Id].ToArray()))
            .ToArray();

        if (!latitudOrigen.HasValue || !longitudOrigen.HasValue)
        {
            return ubicaciones.OrderBy(refugio => refugio.Nombre, StringComparer.CurrentCultureIgnoreCase).ToArray();
        }

        return ubicaciones
            .OrderBy(refugio => CalcularDistanciaKm(latitudOrigen.Value, longitudOrigen.Value, refugio.Latitud, refugio.Longitud))
            .ThenBy(refugio => refugio.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    /// <summary>Calcula distancia de círculo máximo entre dos coordenadas en kilómetros.</summary>
    public static double CalcularDistanciaKm(decimal latitudA, decimal longitudA, decimal latitudB, decimal longitudB)
    {
        var radianes = Math.PI / 180d;
        var latA = (double)latitudA * radianes;
        var latB = (double)latitudB * radianes;
        var diferenciaLatitud = ((double)(latitudB - latitudA)) * radianes;
        var diferenciaLongitud = ((double)(longitudB - longitudA)) * radianes;
        var a = Math.Pow(Math.Sin(diferenciaLatitud / 2d), 2d)
            + Math.Cos(latA) * Math.Cos(latB) * Math.Pow(Math.Sin(diferenciaLongitud / 2d), 2d);
        return RadioTierraKm * 2d * Math.Asin(Math.Min(1d, Math.Sqrt(a)));
    }

    private static bool TieneCoordenadasValidas(Refugio refugio) =>
        refugio.Latitud is >= -90m and <= 90m
        && refugio.Longitud is >= -180m and <= 180m;

    private static RefugioMapaDto CrearDto(Refugio refugio, IReadOnlyCollection<Mascota> mascotas) => new(
        refugio.Id,
        refugio.Nombre,
        refugio.Direccion,
        refugio.Ciudad,
        refugio.Horario,
        refugio.Telefono,
        refugio.FotoUrl,
        refugio.Latitud!.Value,
        refugio.Longitud!.Value,
        mascotas.Count,
        mascotas.Select(mascota => mascota.Especie).Distinct().OrderBy(especie => especie).ToArray());
}
