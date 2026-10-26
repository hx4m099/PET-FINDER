using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetFinder.Entity.Constants;
using PetFinder.Entity.Entities;
using PetFinder.Datos.Seed;

namespace PetFinder.Datos.Configurations;

/// <summary>Mapeo relacional de citas de adopción.</summary>
public sealed class CitaConfiguration : IEntityTypeConfiguration<Cita>
{
    public void Configure(EntityTypeBuilder<Cita> builder)
    {
        builder.ToTable("CITAS");
        builder.HasKey(cita => cita.Id);
        builder.Property(cita => cita.Estado).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(cita => cita.Notas).HasMaxLength(LimitesEntidad.Notas).IsRequired();
        builder.HasIndex(cita => new { cita.RefugioId, cita.FechaInicioUtc, cita.FechaFinUtc });
        builder.HasIndex(cita => cita.UsuarioId);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(cita => cita.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Refugio>().WithMany().HasForeignKey(cita => cita.RefugioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Mascota>().WithMany().HasForeignKey(cita => cita.MascotaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasData(SeedData.Citas.Cast<object>());
    }
}
