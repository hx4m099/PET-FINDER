using FluentValidation;
using PetFinder.BLL.Dtos;
using PetFinder.BLL.Exceptions;
using PetFinder.DAL.Interfaces;
using PetFinder.Entity.Entities;
using PetFinder.Entity.Enums;

namespace PetFinder.BLL.Services;

/// <summary>Orquesta el filtrado geográfico/temporal y la comparación de imágenes.</summary>
public sealed class CoincidenciaService(
    IReporteRepository reporteRepository,
    IImageSimilarityService imageSimilarityService,
    IValidator<BuscarCoincidenciasDto> validator)
{
    /// <summary>Busca reportes opuestos recientes; especie, ciudad y fecha se filtran antes del mock visual.</summary>
    public async Task<IReadOnlyList<CoincidenciaDto>> BuscarParaReporteAsync(
        Guid reporteId,
        int diasRecientes = 30,
        CancellationToken cancellationToken = default)
    {
        if (reporteId == Guid.Empty)
        {
            throw new BusinessRuleException("Se requiere un identificador de reporte válido.");
        }

        var origen = await reporteRepository.ObtenerPorIdAsync(reporteId, cancellationToken)
            ?? throw new EntityNotFoundException("reporte", reporteId);
        var solicitud = new BuscarCoincidenciasDto
        {
            Especie = origen.Especie,
            Ciudad = origen.Ciudad,
            ExcluirReporteId = origen.Id,
            TipoReporteOrigen = origen.Tipo,
            DiasRecientes = diasRecientes
        };
        await validator.ValidateAndThrowAsync(solicitud, cancellationToken);

        var fechaDesdeUtc = origen.FechaReporteUtc.AddDays(-diasRecientes);
        var candidatos = await reporteRepository.ObtenerCandidatosAsync(
            origen.Especie,
            origen.Ciudad,
            fechaDesdeUtc,
            cancellationToken);

        var compatibles = candidatos
            .Where(candidato => candidato.Id != origen.Id
                && candidato.Tipo != origen.Tipo
                && candidato.Activo
                && !string.IsNullOrWhiteSpace(candidato.FotoUrl))
            .ToArray();

        var resultados = new List<CoincidenciaDto>(compatibles.Length);
        foreach (var candidato in compatibles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var porcentaje = await imageSimilarityService.CalcularPorcentajeAsync(
                origen.FotoUrl,
                candidato.FotoUrl,
                cancellationToken);
            resultados.Add(new CoincidenciaDto(candidato, Math.Clamp(porcentaje, 0m, 100m)));
        }

        return resultados
            .OrderByDescending(resultado => resultado.PorcentajeSimilitud)
            .ThenBy(resultado => resultado.Reporte.FechaReporteUtc)
            .ToArray();
    }

    /// <summary>Busca candidatos para un reporte aún no persistido usando la misma reducción previa.</summary>
    public async Task<IReadOnlyList<CoincidenciaDto>> BuscarAsync(
        BuscarCoincidenciasDto solicitud,
        string fotoReferencia,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(solicitud);
        await validator.ValidateAndThrowAsync(solicitud, cancellationToken);
        if (string.IsNullOrWhiteSpace(fotoReferencia))
        {
            throw new BusinessRuleException("La foto de referencia es obligatoria.");
        }

        var fechaDesdeUtc = DateTime.UtcNow.AddDays(-solicitud.DiasRecientes);
        var candidatos = await reporteRepository.ObtenerCandidatosAsync(
            solicitud.Especie,
            solicitud.Ciudad.Trim(),
            fechaDesdeUtc,
            cancellationToken);
        var compatibles = candidatos
            .Where(candidato => candidato.Id != solicitud.ExcluirReporteId
                && (!solicitud.TipoReporteOrigen.HasValue || candidato.Tipo != solicitud.TipoReporteOrigen.Value)
                && !string.IsNullOrWhiteSpace(candidato.FotoUrl))
            .ToArray();

        var resultados = new List<CoincidenciaDto>(compatibles.Length);
        foreach (var candidato in compatibles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var porcentaje = await imageSimilarityService.CalcularPorcentajeAsync(
                fotoReferencia,
                candidato.FotoUrl,
                cancellationToken);
            resultados.Add(new CoincidenciaDto(candidato, Math.Clamp(porcentaje, 0m, 100m)));
        }

        return resultados.OrderByDescending(resultado => resultado.PorcentajeSimilitud).ToArray();
    }
}
