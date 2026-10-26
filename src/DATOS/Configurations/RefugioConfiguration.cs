using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetFinder.Entity.Constants;
using PetFinder.Entity.Entities;
using PetFinder.Datos.Seed;

namespace PetFinder.Datos.Configurations;

/// <summary>Mapeo relacional de refugios aliados.</summary>
public sealed class RefugioConfiguration : IEntityTypeConfiguration<Refugio>
{
    public void Configure(EntityTypeBuilder<Refugio> builder)
    {
        builder.ToTable("REFUGIOS");
        builder.HasKey(refugio => refugio.Id);
        builder.Property(refugio => refugio.Nombre).HasMaxLength(LimitesEntidad.Nombre).IsRequired();
        builder.Property(refugio => refugio.Descripcion).HasMaxLength(LimitesEntidad.Descripcion).IsRequired();
        builder.Property(refugio => refugio.Ciudad).HasMaxLength(LimitesEntidad.Ciudad).IsRequired();
        builder.Property(refugio => refugio.Direccion).HasMaxLength(LimitesEntidad.Direccion).IsRequired();
        builder.Property(refugio => refugio.Telefono).HasMaxLength(LimitesEntidad.Telefono).IsRequired();
        builder.Property(refugio => refugio.CorreoElectronico).HasMaxLength(LimitesEntidad.CorreoElectronico).IsRequired();
        builder.HasIndex(refugio => refugio.UsuarioId).IsUnique();
        builder.HasIndex(refugio => refugio.CorreoElectronico).IsUnique();
        builder.HasOne<Usuario>().WithMany().HasForeignKey(refugio => refugio.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasData(SeedData.Refugios.Cast<object>());
    }
}
