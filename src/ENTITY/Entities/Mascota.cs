namespace PetFinder.Entity.Entities;

/// <summary>Representa una mascota publicada por un refugio.</summary>
public sealed class Mascota
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RefugioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Enums.Especie Especie { get; set; }
    public string Raza { get; set; } = string.Empty;
    public int? EdadMeses { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string FotoUrl { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public bool DisponibleAdopcion { get; set; } = true;
    public bool Activa { get; set; } = true;
    public DateTime FechaPublicacionUtc { get; set; } = DateTime.UtcNow;
}
