using PetFinder.Entity.Enums;

namespace PetFinder.BLL.Dtos;

/// <summary>Datos autorizados para publicar o actualizar una mascota del refugio.</summary>
public sealed record GuardarMascotaRefugioDto(
    Guid? Id,
    string Nombre,
    Especie Especie,
    string Raza,
    int? EdadMeses,
    string Descripcion,
    string FotoUrl,
    string Ciudad,
    bool DisponibleAdopcion);
