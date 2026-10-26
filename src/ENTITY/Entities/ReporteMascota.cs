namespace PetFinder.Entity.Entities;

/// <summary>Representa un reporte de una mascota perdida o encontrada.</summary>
public sealed class ReporteMascota
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public Guid? MascotaId { get; set; }
    public Enums.TipoReporte Tipo { get; set; }
    public Enums.Especie Especie { get; set; }
    public string NombreMascota { get; set; } = string.Empty;
    public string Raza { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string FotoUrl { get; set; } = string.Empty;
    public DateTime FechaEventoUtc { get; set; }
    public DateTime FechaReporteUtc { get; set; } = DateTime.UtcNow;
    public bool MascotaReunida { get; set; }
    public bool Activo { get; set; } = true;
}
