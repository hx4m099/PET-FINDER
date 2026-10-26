using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetFinder.Entity.Constants;
using PetFinder.Entity.Entities;
using PetFinder.Datos.Seed;

namespace PetFinder.Datos.Configurations;

/// <summary>Mapeo relacional de usuarios.</summary>
public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("USUARIOS");
        builder.HasKey(usuario => usuario.Id);
        builder.Property(usuario => usuario.NombreCompleto).HasMaxLength(LimitesEntidad.Nombre).IsRequired();
        builder.Property(usuario => usuario.CorreoElectronico).HasMaxLength(LimitesEntidad.CorreoElectronico).IsRequired();
        builder.Property(usuario => usuario.PasswordHash).HasMaxLength(LimitesEntidad.PasswordHash).IsRequired();
        builder.Property(usuario => usuario.Rol).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(usuario => usuario.Activo).IsRequired();
        builder.Property(usuario => usuario.FechaRegistroUtc).IsRequired();
        builder.HasIndex(usuario => usuario.CorreoElectronico).IsUnique();
        builder.HasData(SeedData.Usuarios.Cast<object>());
    }
}
