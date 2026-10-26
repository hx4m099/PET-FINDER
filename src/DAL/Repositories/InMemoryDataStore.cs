using System.Collections.Concurrent;
using PetFinder.Datos.Seed;
using PetFinder.Entity.Entities;

namespace PetFinder.DAL.Repositories;

/// <summary>Almacén concurrente compartido por las implementaciones en memoria.</summary>
public sealed class InMemoryDataStore
{
    internal ConcurrentDictionary<Guid, Usuario> Usuarios { get; } =
        new(SeedData.Usuarios.ToDictionary(usuario => usuario.Id));

    internal ConcurrentDictionary<Guid, Refugio> Refugios { get; } =
        new(SeedData.Refugios.ToDictionary(refugio => refugio.Id));

    internal ConcurrentDictionary<Guid, Mascota> Mascotas { get; } =
        new(SeedData.Mascotas.ToDictionary(mascota => mascota.Id));

    internal ConcurrentDictionary<Guid, ReporteMascota> Reportes { get; } =
        new(SeedData.ReportesMascota.ToDictionary(reporte => reporte.Id));

    internal ConcurrentDictionary<Guid, Cita> Citas { get; } =
        new(SeedData.Citas.ToDictionary(cita => cita.Id));
}
