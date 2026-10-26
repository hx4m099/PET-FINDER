using PetFinder.Entity.Enums;

namespace PetFinder.BLL.Dtos;

/// <summary>Información pública, ya validada, que alimenta el mapa de refugios.</summary>
public sealed record RefugioMapaDto(
    Guid Id,
    string Nombre,
    string Direccion,
    string Ciudad,
    string Horario,
    string Telefono,
    string FotoUrl,
    decimal Latitud,
    decimal Longitud,
    int MascotasDisponibles,
    IReadOnlyCollection<Especie> EspeciesDisponibles);
