using System.Collections.Concurrent;
using FluentValidation;
using Microsoft.Extensions.Logging;
using PetFinder.BLL.Dtos;
using PetFinder.BLL.Exceptions;
using PetFinder.DAL.Interfaces;
using PetFinder.Entity.Entities;
using PetFinder.Entity.Enums;

namespace PetFinder.BLL.Services;

/// <summary>Orquesta solicitudes, conflictos y transiciones del ciclo de vida de citas.</summary>
public sealed class CitaService(
    ICitaRepository citaRepository,
    IUsuarioRepository usuarioRepository,
    IRefugioRepository refugioRepository,
    IMascotaRepository mascotaRepository,
    IValidator<SolicitarCitaDto> validator,
    TimeProvider timeProvider,
    ILogger<CitaService> logger)
{
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> BloqueosPorRefugio = new();

    /// <summary>Solicita una cita si el refugio, la mascota y el horario están disponibles.</summary>
    public async Task<Cita> SolicitarAsync(SolicitarCitaDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        await validator.ValidateAndThrowAsync(dto, cancellationToken);

        var usuario = await usuarioRepository.ObtenerPorIdAsync(dto.UsuarioId, cancellationToken);
        if (usuario is null || !usuario.Activo)
        {
            throw new EntityNotFoundException("usuario", dto.UsuarioId);
        }

        var refugio = await refugioRepository.ObtenerPorIdAsync(dto.RefugioId, cancellationToken);
        if (refugio is null || !refugio.Activo)
        {
            throw new EntityNotFoundException("refugio activo", dto.RefugioId);
        }

        var mascota = await mascotaRepository.ObtenerPorIdAsync(dto.MascotaId, cancellationToken);
        if (mascota is null || !mascota.Activa || !mascota.DisponibleAdopcion || mascota.RefugioId != refugio.Id)
        {
            throw new BusinessRuleException("La mascota no está disponible en el refugio seleccionado.");
        }

        if (dto.FechaInicioUtc.Kind != DateTimeKind.Utc)
        {
            throw new BusinessRuleException("La fecha de inicio debe estar expresada en UTC.");
        }

        var inicioUtc = dto.FechaInicioUtc;
        var finUtc = inicioUtc.AddMinutes(dto.DuracionMinutos);
        if (inicioUtc <= timeProvider.GetUtcNow().UtcDateTime)
        {
            throw new BusinessRuleException("La cita debe programarse para una fecha futura.");
        }

        var bloqueo = BloqueosPorRefugio.GetOrAdd(dto.RefugioId, _ => new SemaphoreSlim(1, 1));
        await bloqueo.WaitAsync(cancellationToken);
        try
        {
            var cruces = await citaRepository.ObtenerCrucesDeHorarioAsync(
                dto.RefugioId,
                inicioUtc,
                finUtc,
                cancellationToken: cancellationToken);
            if (cruces.Count > 0)
            {
                throw new BusinessRuleException("El horario se cruza con otra cita pendiente o confirmada del refugio.");
            }

            var cita = new Cita
            {
                UsuarioId = dto.UsuarioId,
                RefugioId = dto.RefugioId,
                MascotaId = dto.MascotaId,
                FechaInicioUtc = inicioUtc,
                FechaFinUtc = finUtc,
                Estado = EstadoCita.Pendiente,
                FechaCreacionUtc = timeProvider.GetUtcNow().UtcDateTime,
                Notas = dto.Notas.Trim()
            };
            await citaRepository.GuardarAsync(cita, cancellationToken);
            logger.LogInformation("Se creó la cita {CitaId} para el refugio {RefugioId}.", cita.Id, cita.RefugioId);
            return cita;
        }
        finally
        {
            bloqueo.Release();
        }
    }

    /// <summary>Obtiene las citas de un usuario ordenadas por fecha.</summary>
    public Task<IReadOnlyCollection<Cita>> ObtenerPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        if (usuarioId == Guid.Empty)
        {
            throw new BusinessRuleException("Se requiere un identificador de usuario válido.");
        }

        return citaRepository.ObtenerPorUsuarioAsync(usuarioId, cancellationToken);
    }

    /// <summary>Obtiene las solicitudes recibidas por el refugio ligado a la cuenta autenticada.</summary>
    public async Task<IReadOnlyCollection<Cita>> ObtenerPorRefugioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken);
        if (usuario is null || !usuario.Activo)
        {
            throw new EntityNotFoundException("usuario", usuarioId);
        }

        if (usuario.Rol != Rol.Refugio)
        {
            throw new BusinessRuleException("Esta vista requiere una cuenta de refugio.");
        }

        var refugio = (await refugioRepository.ObtenerTodosAsync(cancellationToken: cancellationToken))
            .FirstOrDefault(item => item.UsuarioId == usuarioId && item.Activo)
            ?? throw new EntityNotFoundException("refugio activo", usuarioId);
        return await citaRepository.ObtenerPorRefugioYFechaAsync(
            refugio.Id,
            DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc),
            DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc),
            cancellationToken);
    }

    /// <summary>Confirma una cita pendiente únicamente para el refugio asociado o un administrador.</summary>
    public async Task<Cita> ConfirmarAsync(Guid citaId, Guid usuarioActorId, CancellationToken cancellationToken = default)
    {
        var cita = await ObtenerCitaRequeridaAsync(citaId, cancellationToken);
        var actor = await usuarioRepository.ObtenerPorIdAsync(usuarioActorId, cancellationToken);
        if (actor is null || !actor.Activo)
        {
            throw new EntityNotFoundException("usuario", usuarioActorId);
        }

        var esAdministrador = actor.Rol == Rol.Administrador;
        var refugioActor = actor.Rol == Rol.Refugio
            ? (await refugioRepository.ObtenerTodosAsync(cancellationToken: cancellationToken))
                .FirstOrDefault(refugio => refugio.UsuarioId == actor.Id && refugio.Id == cita.RefugioId && refugio.Activo)
            : null;
        if (!esAdministrador && refugioActor is null)
        {
            throw new BusinessRuleException("Solo el refugio de la cita o un administrador puede confirmarla.");
        }

        if (cita.Estado != EstadoCita.Pendiente)
        {
            throw new BusinessRuleException("Solo se pueden confirmar citas pendientes.");
        }

        cita.Estado = EstadoCita.Confirmada;
        await citaRepository.GuardarAsync(cita, cancellationToken);
        logger.LogInformation("La cita {CitaId} fue confirmada por el usuario {UsuarioId}.", cita.Id, actor.Id);
        return cita;
    }

    /// <summary>Cancela una cita pendiente o confirmada por su adoptante, su refugio o un administrador.</summary>
    public async Task<Cita> CancelarAsync(Guid citaId, Guid usuarioActorId, CancellationToken cancellationToken = default)
    {
        var cita = await ObtenerCitaRequeridaAsync(citaId, cancellationToken);
        var actor = await usuarioRepository.ObtenerPorIdAsync(usuarioActorId, cancellationToken);
        if (actor is null || !actor.Activo)
        {
            throw new EntityNotFoundException("usuario", usuarioActorId);
        }

        var esAdoptantePropietario = cita.UsuarioId == actor.Id;
        var esAdministrador = actor.Rol == Rol.Administrador;
        var esRefugioPropietario = actor.Rol == Rol.Refugio
            && (await refugioRepository.ObtenerTodosAsync(cancellationToken: cancellationToken))
                .Any(refugio => refugio.UsuarioId == actor.Id && refugio.Id == cita.RefugioId && refugio.Activo);
        if (!esAdoptantePropietario && !esAdministrador && !esRefugioPropietario)
        {
            throw new BusinessRuleException("No tienes permiso para cancelar esta cita.");
        }

        if (cita.Estado is not (EstadoCita.Pendiente or EstadoCita.Confirmada))
        {
            throw new BusinessRuleException("Solo se pueden cancelar citas pendientes o confirmadas.");
        }

        cita.Estado = EstadoCita.Cancelada;
        await citaRepository.GuardarAsync(cita, cancellationToken);
        logger.LogInformation("La cita {CitaId} fue cancelada por el usuario {UsuarioId}.", cita.Id, actor.Id);
        return cita;
    }

    private async Task<Cita> ObtenerCitaRequeridaAsync(Guid citaId, CancellationToken cancellationToken)
    {
        if (citaId == Guid.Empty)
        {
            throw new BusinessRuleException("Se requiere un identificador de cita válido.");
        }

        return await citaRepository.ObtenerPorIdAsync(citaId, cancellationToken)
            ?? throw new EntityNotFoundException("cita", citaId);
    }
}
