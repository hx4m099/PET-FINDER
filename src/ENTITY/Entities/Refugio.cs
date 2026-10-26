namespace PetFinder.Entity.Entities;

/// <summary>Representa un refugio aliado registrado en PetFinder.</summary>
public sealed class Refugio
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Horario { get; set; } = string.Empty;
    public string FotoUrl { get; set; } = string.Empty;
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
    public string CorreoElectronico { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistroUtc { get; set; } = DateTime.UtcNow;
}
