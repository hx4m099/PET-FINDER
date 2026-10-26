using PetFinder.Entity.Enums;

namespace PetFinder.BLL.Dtos;

/// <summary>Identidad no sensible devuelta tras autenticación correcta.</summary>
public sealed record SesionUsuarioDto(Guid Id, string NombreCompleto, string CorreoElectronico, Rol Rol);
