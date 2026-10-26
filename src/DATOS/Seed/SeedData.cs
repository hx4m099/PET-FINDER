using PetFinder.Entity.Entities;
using PetFinder.Entity.Enums;

namespace PetFinder.Datos.Seed;

/// <summary>Datos deterministas de demostración compartidos por EF Core y los repositorios en memoria.</summary>
public static class SeedData
{
    public static IReadOnlyList<Usuario> Usuarios { get; } =
    [
        new Usuario
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            NombreCompleto = "Administración PetFinder",
            CorreoElectronico = "admin@petfinder.local",
            PasswordHash = string.Empty,
            Rol = Rol.Administrador,
            Activo = true,
            FechaRegistroUtc = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc)
        },
        new Usuario
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            NombreCompleto = "Equipo Huellitas",
            CorreoElectronico = "refugio@petfinder.local",
            PasswordHash = string.Empty,
            Rol = Rol.Refugio,
            Activo = true,
            FechaRegistroUtc = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc)
        },
        new Usuario
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            NombreCompleto = "Adoptante de ejemplo",
            CorreoElectronico = "adoptante@petfinder.local",
            PasswordHash = string.Empty,
            Rol = Rol.Adoptante,
            Activo = true,
            FechaRegistroUtc = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc)
        }
    ];

    public static IReadOnlyList<Refugio> Refugios { get; } =
    [
        new Refugio
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            UsuarioId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Nombre = "Huellitas Valledupar",
            Descripcion = "Refugio aliado dedicado al cuidado y adopción responsable.",
            Ciudad = "Valledupar",
            Direccion = "Calle 12 # 8-40",
            Telefono = "+57 300 000 0000",
            CorreoElectronico = "contacto@huellitas.local",
            Activo = true,
            FechaRegistroUtc = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc)
        }
    ];

    public static IReadOnlyList<Mascota> Mascotas { get; } =
    [
        new Mascota
        {
            Id = Guid.Parse("55555555-0000-0000-0000-000000000001"),
            RefugioId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Nombre = "Luna",
            Especie = Especie.Perro,
            Raza = "Criolla",
            EdadMeses = 18,
            Descripcion = "Cariñosa, sociable y lista para encontrar un hogar.",
            FotoUrl = "/images/mascotas/luna.jpg",
            Ciudad = "Valledupar",
            DisponibleAdopcion = true,
            Activa = true,
            FechaPublicacionUtc = new DateTime(2026, 10, 5, 12, 0, 0, DateTimeKind.Utc)
        },
        new Mascota
        {
            Id = Guid.Parse("55555555-0000-0000-0000-000000000002"),
            RefugioId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Nombre = "Milo",
            Especie = Especie.Gato,
            Raza = "Doméstico de pelo corto",
            EdadMeses = 8,
            Descripcion = "Gatito tranquilo, curioso y acostumbrado a interiores.",
            FotoUrl = "/images/mascotas/milo.jpg",
            Ciudad = "Valledupar",
            DisponibleAdopcion = true,
            Activa = true,
            FechaPublicacionUtc = new DateTime(2026, 10, 8, 12, 0, 0, DateTimeKind.Utc)
        },
        new Mascota
        {
            Id = Guid.Parse("55555555-0000-0000-0000-000000000003"),
            RefugioId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Nombre = "Rocky",
            Especie = Especie.Perro,
            Raza = "Labrador mestizo",
            EdadMeses = 36,
            Descripcion = "Perro activo que disfruta los paseos y la compañía.",
            FotoUrl = "/images/mascotas/rocky.jpg",
            Ciudad = "Valledupar",
            DisponibleAdopcion = true,
            Activa = true,
            FechaPublicacionUtc = new DateTime(2026, 10, 12, 12, 0, 0, DateTimeKind.Utc)
        }
    ];

    public static IReadOnlyList<ReporteMascota> ReportesMascota { get; } =
    [
        new ReporteMascota
        {
            Id = Guid.Parse("66666666-0000-0000-0000-000000000001"),
            UsuarioId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Tipo = TipoReporte.Perdida,
            Especie = Especie.Perro,
            NombreMascota = "Toby",
            Raza = "Criollo",
            Ciudad = "Valledupar",
            Descripcion = "Perro café con pecho blanco, visto cerca del parque.",
            FotoUrl = "/images/reportes/toby-perdido.jpg",
            FechaEventoUtc = new DateTime(2026, 10, 18, 16, 0, 0, DateTimeKind.Utc),
            FechaReporteUtc = new DateTime(2026, 10, 19, 9, 0, 0, DateTimeKind.Utc),
            Activo = true
        },
        new ReporteMascota
        {
            Id = Guid.Parse("66666666-0000-0000-0000-000000000002"),
            UsuarioId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Tipo = TipoReporte.Encontrada,
            Especie = Especie.Perro,
            NombreMascota = string.Empty,
            Raza = "Criollo",
            Ciudad = "Valledupar",
            Descripcion = "Perro café encontrado cerca del parque principal.",
            FotoUrl = "/images/reportes/perro-encontrado.jpg",
            FechaEventoUtc = new DateTime(2026, 10, 20, 16, 0, 0, DateTimeKind.Utc),
            FechaReporteUtc = new DateTime(2026, 10, 20, 18, 0, 0, DateTimeKind.Utc),
            Activo = true
        },
        new ReporteMascota
        {
            Id = Guid.Parse("66666666-0000-0000-0000-000000000003"),
            UsuarioId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Tipo = TipoReporte.Perdida,
            Especie = Especie.Gato,
            NombreMascota = "Nube",
            Raza = "Doméstico",
            Ciudad = "Valledupar",
            Descripcion = "Gata gris perdida en el barrio Novalito.",
            FotoUrl = "/images/reportes/nube-perdida.jpg",
            FechaEventoUtc = new DateTime(2026, 10, 21, 8, 0, 0, DateTimeKind.Utc),
            FechaReporteUtc = new DateTime(2026, 10, 21, 10, 0, 0, DateTimeKind.Utc),
            Activo = true
        }
    ];

    public static IReadOnlyList<Cita> Citas { get; } =
    [
        new Cita
        {
            Id = Guid.Parse("77777777-0000-0000-0000-000000000001"),
            UsuarioId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            RefugioId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            MascotaId = Guid.Parse("55555555-0000-0000-0000-000000000001"),
            FechaInicioUtc = new DateTime(2026, 10, 30, 14, 0, 0, DateTimeKind.Utc),
            FechaFinUtc = new DateTime(2026, 10, 30, 14, 30, 0, DateTimeKind.Utc),
            Estado = EstadoCita.Pendiente,
            FechaCreacionUtc = new DateTime(2026, 10, 22, 9, 0, 0, DateTimeKind.Utc),
            Notas = "Cita de ejemplo para demostración."
        }
    ];
}
