using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetFinder.Entity.Constants;
using PetFinder.Entity.Entities;
using PetFinder.Datos.Seed;

namespace PetFinder.Datos.Configurations;

/// <summary>Mapeo relacional de reportes de mascotas.</summary>
public sealed class ReporteMascotaConfiguration : IEntityTypeConfiguration<ReporteMascota>
{
    public void Configure(EntityTypeBuilder<ReporteMascota> builder)
    {
        builder.ToTable("REPORTES_MASCOTA");
        builder.HasKey(reporte => reporte.Id);
        builder.Property(reporte => reporte.Tipo).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(reporte => reporte.Especie).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(reporte => reporte.NombreMascota).HasMaxLength(LimitesEntidad.Nombre).IsRequired();
        builder.Property(reporte => reporte.Raza).HasMaxLength(LimitesEntidad.Raza).IsRequired();
        builder.Property(reporte => reporte.Ciudad).HasMaxLength(LimitesEntidad.Ciudad).IsRequired();
        builder.Property(reporte => reporte.Descripcion).HasMaxLength(LimitesEntidad.Descripcion).IsRequired();
        builder.Property(reporte => reporte.FotoUrl).HasMaxLength(LimitesEntidad.Url).IsRequired();
        builder.HasIndex(reporte => new { reporte.Especie, reporte.Ciudad, reporte.FechaReporteUtc });
        builder.HasIndex(reporte => reporte.UsuarioId);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(reporte => reporte.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Mascota>().WithMany().HasForeignKey(reporte => reporte.MascotaId).OnDelete(DeleteBehavior.SetNull);
        builder.HasData(SeedData.ReportesMascota.Cast<object>());
    }
}
