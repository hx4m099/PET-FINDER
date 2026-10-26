using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetFinder.Entity.Constants;
using PetFinder.Entity.Entities;
using PetFinder.Datos.Seed;

namespace PetFinder.Datos.Configurations;

/// <summary>Mapeo relacional de mascotas publicadas.</summary>
public sealed class MascotaConfiguration : IEntityTypeConfiguration<Mascota>
{
    public void Configure(EntityTypeBuilder<Mascota> builder)
    {
        builder.ToTable("MASCOTAS");
        builder.HasKey(mascota => mascota.Id);
        builder.Property(mascota => mascota.Nombre).HasMaxLength(LimitesEntidad.Nombre).IsRequired();
        builder.Property(mascota => mascota.Especie).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(mascota => mascota.Raza).HasMaxLength(LimitesEntidad.Raza).IsRequired();
        builder.Property(mascota => mascota.Descripcion).HasMaxLength(LimitesEntidad.Descripcion).IsRequired();
        builder.Property(mascota => mascota.FotoUrl).HasMaxLength(LimitesEntidad.Url).IsRequired();
        builder.Property(mascota => mascota.Ciudad).HasMaxLength(LimitesEntidad.Ciudad).IsRequired();
        builder.Property(mascota => mascota.Adoptada).IsRequired();
        builder.HasIndex(mascota => new { mascota.Ciudad, mascota.Especie, mascota.DisponibleAdopcion });
        builder.HasIndex(mascota => mascota.Adoptada);
        builder.HasOne<Refugio>().WithMany().HasForeignKey(mascota => mascota.RefugioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasData(SeedData.Mascotas.Cast<object>());
    }
}
