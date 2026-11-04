using System.ComponentModel.DataAnnotations;
using PetFinder.Entity.Constants;

namespace PetFinder.BLL.Dtos;

/// <summary>Datos para solicitar una cita de adopción en un refugio.</summary>
public sealed class SolicitarCitaDto
{
    [Required]
    public Guid UsuarioId { get; init; }

    [Required]
    public Guid RefugioId { get; init; }

    [Required]
    public Guid MascotaId { get; init; }

    public DateTime FechaInicioUtc { get; init; }

    [Range(15, 120)]
    public int DuracionMinutos { get; init; } = 30;

    [StringLength(LimitesEntidad.Notas)]
    public string Notas { get; init; } = string.Empty;
}
