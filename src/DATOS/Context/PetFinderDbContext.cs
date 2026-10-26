using Microsoft.EntityFrameworkCore;
using PetFinder.Entity.Entities;

namespace PetFinder.Datos.Context;

/// <summary>Contexto de EF Core listo para configurar el proveedor de base de datos en una fase posterior.</summary>
public sealed class PetFinderDbContext(DbContextOptions<PetFinderDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Refugio> Refugios => Set<Refugio>();
    public DbSet<Mascota> Mascotas => Set<Mascota>();
    public DbSet<ReporteMascota> ReportesMascota => Set<ReporteMascota>();
    public DbSet<Cita> Citas => Set<Cita>();

    /// <summary>Aplica las configuraciones Fluent API del ensamblado de DATOS.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PetFinderDbContext).Assembly);
    }
}
