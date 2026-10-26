namespace PetFinder.Entity.Entities;

/// <summary>Representa una cita de adopción en un refugio.</summary>
public sealed class Cita
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public Guid RefugioId { get; set; }
    public Guid MascotaId { get; set; }
    public DateTime FechaInicioUtc { get; set; }
    public DateTime FechaFinUtc { get; set; }
    public Enums.EstadoCita Estado { get; set; } = Enums.EstadoCita.Pendiente;
    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
    public string Notas { get; set; } = string.Empty;
}
