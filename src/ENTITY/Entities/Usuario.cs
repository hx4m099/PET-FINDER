namespace PetFinder.Entity.Entities;

/// <summary>Representa una cuenta de usuario de PetFinder.</summary>
public sealed class Usuario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string NombreCompleto { get; set; } = string.Empty;
    public string CorreoElectronico { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Enums.Rol Rol { get; set; } = Enums.Rol.Adoptante;
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistroUtc { get; set; } = DateTime.UtcNow;
}
