using PetFinder.Entity.Entities;

namespace PetFinder.DAL.Interfaces;

/// <summary>Abstracción de persistencia para citas de adopción.</summary>
public interface ICitaRepository
{
    /// <summary>Busca una cita por su identificador.</summary>
    Task<Cita?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Obtiene las citas de un usuario.</summary>
    Task<IReadOnlyCollection<Cita>> ObtenerPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);

    /// <summary>Obtiene las citas de un refugio cuyo inicio cae dentro del rango indicado.</summary>
    Task<IReadOnlyCollection<Cita>> ObtenerPorRefugioYFechaAsync(
        Guid refugioId,
        DateTime inicioUtc,
        DateTime finUtc,
        CancellationToken cancellationToken = default);

    /// <summary>Busca reservas activas que se cruzan con un intervalo, excluyendo opcionalmente una cita.</summary>
    Task<IReadOnlyCollection<Cita>> ObtenerCrucesDeHorarioAsync(
        Guid refugioId,
        DateTime inicioUtc,
        DateTime finUtc,
        Guid? excluirCitaId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Inserta o actualiza una cita.</summary>
    Task GuardarAsync(Cita cita, CancellationToken cancellationToken = default);
}
