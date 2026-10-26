using FluentValidation;
using Microsoft.Extensions.Logging;
using PetFinder.BLL.Dtos;
using PetFinder.BLL.Exceptions;
using PetFinder.DAL.Interfaces;
using PetFinder.Entity.Entities;

namespace PetFinder.BLL.Services;

/// <summary>Gestiona la publicación y consulta de reportes de mascotas.</summary>
public sealed class ReporteService(
    IReporteRepository reporteRepository,
    IUsuarioRepository usuarioRepository,
    IValidator<CrearReporteMascotaDto> validator,
    TimeProvider timeProvider,
    ILogger<ReporteService> logger)
{
    /// <summary>Publica un reporte asociado al usuario existente que lo envía.</summary>
    public async Task<ReporteMascota> CrearAsync(Guid usuarioId, CrearReporteMascotaDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (usuarioId == Guid.Empty)
        {
            throw new BusinessRuleException("Se requiere un usuario autenticado para crear el reporte.");
        }

        await validator.ValidateAndThrowAsync(dto, cancellationToken);
        var usuario = await usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken);
        if (usuario is null || !usuario.Activo)
        {
            throw new EntityNotFoundException("usuario", usuarioId);
        }

        var fechaEventoUtc = dto.FechaEventoUtc.Kind == DateTimeKind.Utc
            ? dto.FechaEventoUtc
            : DateTime.SpecifyKind(dto.FechaEventoUtc, DateTimeKind.Utc);
        if (fechaEventoUtc > timeProvider.GetUtcNow().UtcDateTime)
        {
            throw new BusinessRuleException("La fecha del evento no puede estar en el futuro.");
        }

        var reporte = new ReporteMascota
        {
            UsuarioId = usuarioId,
            Tipo = dto.Tipo,
            Especie = dto.Especie,
            NombreMascota = dto.NombreMascota.Trim(),
            Raza = dto.Raza.Trim(),
            Ciudad = dto.Ciudad.Trim(),
            Descripcion = dto.Descripcion.Trim(),
            FotoUrl = dto.FotoUrl.Trim(),
            FechaEventoUtc = fechaEventoUtc,
            FechaReporteUtc = timeProvider.GetUtcNow().UtcDateTime,
            Activo = true
        };

        await reporteRepository.GuardarAsync(reporte, cancellationToken);
        logger.LogInformation("Se publicó el reporte {ReporteId} de tipo {Tipo}.", reporte.Id, reporte.Tipo);
        return reporte;
    }

    /// <summary>Obtiene los reportes pertenecientes a un usuario.</summary>
    public Task<IReadOnlyCollection<ReporteMascota>> ObtenerPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        if (usuarioId == Guid.Empty)
        {
            throw new BusinessRuleException("Se requiere un identificador de usuario válido.");
        }

        return reporteRepository.ObtenerPorUsuarioAsync(usuarioId, cancellationToken);
    }

    /// <summary>Obtiene avisos activos recientes para el tablero público.</summary>
    public async Task<IReadOnlyCollection<ReporteMascota>> ObtenerTableroAsync(int cantidad = 6, CancellationToken cancellationToken = default)
    {
        if (cantidad is < 1 or > 20)
        {
            throw new BusinessRuleException("La cantidad de avisos debe estar entre 1 y 20.");
        }

        var reportes = await reporteRepository.ObtenerTodosAsync(cancellationToken);
        return reportes
            .Where(reporte => reporte.Activo)
            .OrderByDescending(reporte => reporte.FechaReporteUtc)
            .Take(cantidad)
            .ToArray();
    }
}
