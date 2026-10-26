using PetFinder.BLL.Exceptions;
using PetFinder.DAL.Interfaces;
using PetFinder.Entity.Entities;
using PetFinder.Entity.Enums;

namespace PetFinder.BLL.Services;

/// <summary>Consulta el catálogo de adopción y sus publicaciones disponibles.</summary>
public sealed class AdopcionService(IMascotaRepository mascotaRepository, IRefugioRepository refugioRepository)
{
    /// <summary>Obtiene mascotas disponibles con filtros opcionales de refugio, especie y edad.</summary>
    public async Task<IReadOnlyCollection<Mascota>> ObtenerCatalogoAsync(
        Guid? refugioId = null,
        Especie? especie = null,
        int? edadMinimaMeses = null,
        int? edadMaximaMeses = null,
        CancellationToken cancellationToken = default)
    {
        if (edadMinimaMeses < 0 || edadMaximaMeses < 0
            || edadMinimaMeses.HasValue && edadMaximaMeses.HasValue && edadMinimaMeses > edadMaximaMeses)
        {
            throw new BusinessRuleException("El rango de edad indicado no es válido.");
        }

        var mascotas = await mascotaRepository.ObtenerDisponiblesAdopcionAsync(
            refugioId,
            especie,
            edadMinimaMeses,
            edadMaximaMeses,
            cancellationToken);
        var refugiosActivos = await refugioRepository.ObtenerTodosAsync(true, cancellationToken);
        var idsActivos = refugiosActivos.Select(refugio => refugio.Id).ToHashSet();
        return mascotas.Where(mascota => idsActivos.Contains(mascota.RefugioId)).ToArray();
    }

    /// <summary>Obtiene el detalle de una mascota disponible y su refugio aliado.</summary>
    public async Task<(Mascota Mascota, Refugio Refugio)> ObtenerDetalleAsync(Guid mascotaId, CancellationToken cancellationToken = default)
    {
        var mascota = await mascotaRepository.ObtenerPorIdAsync(mascotaId, cancellationToken)
            ?? throw new EntityNotFoundException("mascota", mascotaId);
        if (!mascota.Activa || !mascota.DisponibleAdopcion)
        {
            throw new EntityNotFoundException("mascota disponible", mascotaId);
        }

        var refugio = await refugioRepository.ObtenerPorIdAsync(mascota.RefugioId, cancellationToken);
        if (refugio is null || !refugio.Activo)
        {
            throw new EntityNotFoundException("refugio activo", mascota.RefugioId);
        }

        return (mascota, refugio);
    }
}
