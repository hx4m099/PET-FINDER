using PetFinder.Entity.Entities;
using PetFinder.Entity.Enums;

namespace PetFinder.Datos.Seed;

/// <summary>Datos deterministas de demostración compartidos por EF Core y los repositorios en memoria.</summary>
public static class SeedData
{
    private const string FotoPerroA = "https://files.manuscdn.com/search-media/310519664044868707/VewYamXnKlaJIfmZIqVu1T/vgZnPfb8rApRAsz2uT4jzZ.jpg";
    private const string FotoGatoA = "https://files.manuscdn.com/search-media/310519664044868707/VewYamXnKlaJIfmZIqVu1T/KcYegWEBdwbfnX48bS5gBP.jpg";
    private const string FotoPerroB = "https://files.manuscdn.com/search-media/310519664044868707/VewYamXnKlaJIfmZIqVu1T/oWdDdYMZQ8bNSFPmWcHCdY.jpg";
    private const string FotoGatoB = "https://files.manuscdn.com/search-media/310519664044868707/VewYamXnKlaJIfmZIqVu1T/FQcSN2K3pKcdfUAugzwSZJ.jpg";
    private const string FotoPerroC = "https://files.manuscdn.com/search-media/310519664044868707/VewYamXnKlaJIfmZIqVu1T/tULjvq4NskR594D5kMT9dC.jpg";
    private const string FotoGatoC = "https://files.manuscdn.com/search-media/310519664044868707/VewYamXnKlaJIfmZIqVu1T/aP9F4n59eu8us5hW6jD4g4.jpg";
    private const string FotoPerroD = "https://files.manuscdn.com/search-media/310519664044868707/VewYamXnKlaJIfmZIqVu1T/hA9FsSehD2jgGxRfzW5pxS.jpg";
    private const string FotoGatoD = "https://files.manuscdn.com/search-media/310519664044868707/VewYamXnKlaJIfmZIqVu1T/tZSBnhuEpQZuoiJ6RXiGXU.jpg";

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
        },
        new Usuario { Id = Guid.Parse("22222222-2222-2222-2222-222222222223"), NombreCompleto = "Equipo Patitas del Cesar", CorreoElectronico = "patitas@petfinder.local", PasswordHash = string.Empty, Rol = Rol.Refugio, Activo = true, FechaRegistroUtc = new DateTime(2026, 10, 2, 0, 0, 0, DateTimeKind.Utc) },
        new Usuario { Id = Guid.Parse("22222222-2222-2222-2222-222222222224"), NombreCompleto = "Equipo Bigotes", CorreoElectronico = "bigotes@petfinder.local", PasswordHash = string.Empty, Rol = Rol.Refugio, Activo = true, FechaRegistroUtc = new DateTime(2026, 10, 3, 0, 0, 0, DateTimeKind.Utc) },
        new Usuario { Id = Guid.Parse("22222222-2222-2222-2222-222222222225"), NombreCompleto = "Rescate Esperanza", CorreoElectronico = "esperanza@petfinder.local", PasswordHash = string.Empty, Rol = Rol.Refugio, Activo = true, FechaRegistroUtc = new DateTime(2026, 10, 4, 0, 0, 0, DateTimeKind.Utc) },
        new Usuario { Id = Guid.Parse("22222222-2222-2222-2222-222222222226"), NombreCompleto = "Manos y Patas", CorreoElectronico = "manosypatas@petfinder.local", PasswordHash = string.Empty, Rol = Rol.Refugio, Activo = true, FechaRegistroUtc = new DateTime(2026, 10, 5, 0, 0, 0, DateTimeKind.Utc) },
        new Usuario { Id = Guid.Parse("22222222-2222-2222-2222-222222222227"), NombreCompleto = "Casa Michi", CorreoElectronico = "casamichi@petfinder.local", PasswordHash = string.Empty, Rol = Rol.Refugio, Activo = true, FechaRegistroUtc = new DateTime(2026, 10, 6, 0, 0, 0, DateTimeKind.Utc) }
    ];

    public static IReadOnlyList<Refugio> Refugios { get; } =
    [
        new Refugio
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            UsuarioId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Nombre = "Huellitas Valledupar",
            Descripcion = "Datos ficticios de demostración. Refugio aliado dedicado al cuidado y adopción responsable.",
            Ciudad = "Valledupar",
            Direccion = "Dirección ficticia, barrio Novalito",
            Telefono = "+57 300 000 0001",
            Horario = "Horario de ejemplo: lun-sáb, 9:00-17:00",
            FotoUrl = FotoPerroA,
            Latitud = 10.4630m,
            Longitud = -73.2530m,
            CorreoElectronico = "contacto@huellitas.local",
            Activo = true,
            FechaRegistroUtc = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc)
        },
        new Refugio { Id = Guid.Parse("44444444-4444-4444-4444-444444444445"), UsuarioId = Guid.Parse("22222222-2222-2222-2222-222222222223"), Nombre = "Patitas del Cesar", Descripcion = "Perfil y ubicación ficticios para demostración.", Ciudad = "Valledupar", Direccion = "Dirección ficticia, barrio Centro", Telefono = "+57 300 000 0002", Horario = "Horario de ejemplo: lun-vie, 8:30-16:30", FotoUrl = FotoPerroB, Latitud = 10.4740m, Longitud = -73.2430m, CorreoElectronico = "contacto@patitas.local", Activo = true, FechaRegistroUtc = new DateTime(2026, 10, 2, 0, 0, 0, DateTimeKind.Utc) },
        new Refugio { Id = Guid.Parse("44444444-4444-4444-4444-444444444446"), UsuarioId = Guid.Parse("22222222-2222-2222-2222-222222222224"), Nombre = "Hogar Bigotes", Descripcion = "Perfil y ubicación ficticios para demostración.", Ciudad = "Valledupar", Direccion = "Dirección ficticia, barrio San Joaquín", Telefono = "+57 300 000 0003", Horario = "Horario de ejemplo: mar-dom, 10:00-18:00", FotoUrl = FotoGatoA, Latitud = 10.4525m, Longitud = -73.2450m, CorreoElectronico = "contacto@bigotes.local", Activo = true, FechaRegistroUtc = new DateTime(2026, 10, 3, 0, 0, 0, DateTimeKind.Utc) },
        new Refugio { Id = Guid.Parse("44444444-4444-4444-4444-444444444447"), UsuarioId = Guid.Parse("22222222-2222-2222-2222-222222222225"), Nombre = "Rescate Esperanza", Descripcion = "Perfil y ubicación ficticios para demostración.", Ciudad = "Valledupar", Direccion = "Dirección ficticia, barrio Las Flores", Telefono = "+57 300 000 0004", Horario = "Horario de ejemplo: lun-sáb, 9:00-16:00", FotoUrl = FotoPerroC, Latitud = 10.4685m, Longitud = -73.2630m, CorreoElectronico = "contacto@esperanza.local", Activo = true, FechaRegistroUtc = new DateTime(2026, 10, 4, 0, 0, 0, DateTimeKind.Utc) },
        new Refugio { Id = Guid.Parse("44444444-4444-4444-4444-444444444448"), UsuarioId = Guid.Parse("22222222-2222-2222-2222-222222222226"), Nombre = "Manos y Patas", Descripcion = "Perfil y ubicación ficticios para demostración.", Ciudad = "Valledupar", Direccion = "Dirección ficticia, barrio Primero de Mayo", Telefono = "+57 300 000 0005", Horario = "Horario de ejemplo: lun-vie, 9:00-17:30", FotoUrl = FotoGatoB, Latitud = 10.4470m, Longitud = -73.2560m, CorreoElectronico = "contacto@manosypatas.local", Activo = true, FechaRegistroUtc = new DateTime(2026, 10, 5, 0, 0, 0, DateTimeKind.Utc) },
        new Refugio { Id = Guid.Parse("44444444-4444-4444-4444-444444444449"), UsuarioId = Guid.Parse("22222222-2222-2222-2222-222222222227"), Nombre = "Casa Michi", Descripcion = "Perfil y ubicación ficticios para demostración.", Ciudad = "Valledupar", Direccion = "Dirección ficticia, barrio Altagracia", Telefono = "+57 300 000 0006", Horario = "Horario de ejemplo: mié-dom, 10:00-17:00", FotoUrl = FotoGatoC, Latitud = 10.4595m, Longitud = -73.2350m, CorreoElectronico = "contacto@casamichi.local", Activo = true, FechaRegistroUtc = new DateTime(2026, 10, 6, 0, 0, 0, DateTimeKind.Utc) }
    ];

    public static IReadOnlyList<Mascota> Mascotas { get; } =
    [
        new Mascota { Id = Guid.Parse("55555555-0000-0000-0000-000000000001"), RefugioId = Guid.Parse("44444444-4444-4444-4444-444444444444"), Nombre = "Luna", Especie = Especie.Perro, Raza = "Criolla", EdadMeses = 18, Descripcion = "Cariñosa, sociable y lista para encontrar un hogar.", FotoUrl = FotoPerroA, Ciudad = "Valledupar", DisponibleAdopcion = true, Activa = true, FechaPublicacionUtc = new DateTime(2026, 10, 5, 12, 0, 0, DateTimeKind.Utc) },
        new Mascota { Id = Guid.Parse("55555555-0000-0000-0000-000000000002"), RefugioId = Guid.Parse("44444444-4444-4444-4444-444444444444"), Nombre = "Milo", Especie = Especie.Gato, Raza = "Doméstico de pelo corto", EdadMeses = 8, Descripcion = "Gatito tranquilo, curioso y acostumbrado a interiores.", FotoUrl = FotoGatoA, Ciudad = "Valledupar", DisponibleAdopcion = true, Activa = true, FechaPublicacionUtc = new DateTime(2026, 10, 8, 12, 0, 0, DateTimeKind.Utc) },
        new Mascota { Id = Guid.Parse("55555555-0000-0000-0000-000000000003"), RefugioId = Guid.Parse("44444444-4444-4444-4444-444444444444"), Nombre = "Rocky", Especie = Especie.Perro, Raza = "Labrador mestizo", EdadMeses = 36, Descripcion = "Perro activo que disfruta los paseos y la compañía.", FotoUrl = FotoPerroB, Ciudad = "Valledupar", DisponibleAdopcion = true, Activa = true, FechaPublicacionUtc = new DateTime(2026, 10, 12, 12, 0, 0, DateTimeKind.Utc) },
        new Mascota { Id = Guid.Parse("55555555-0000-0000-0000-000000000004"), RefugioId = Guid.Parse("44444444-4444-4444-4444-444444444445"), Nombre = "Bella", Especie = Especie.Perro, Raza = "Criolla", EdadMeses = 14, Descripcion = "Juguetona y habituada a paseos tranquilos.", FotoUrl = FotoPerroC, Ciudad = "Valledupar", DisponibleAdopcion = true, Activa = true, FechaPublicacionUtc = new DateTime(2026, 10, 13, 12, 0, 0, DateTimeKind.Utc) },
        new Mascota { Id = Guid.Parse("55555555-0000-0000-0000-000000000005"), RefugioId = Guid.Parse("44444444-4444-4444-4444-444444444446"), Nombre = "Coco", Especie = Especie.Gato, Raza = "Doméstico", EdadMeses = 11, Descripcion = "Curioso, afectuoso y tranquilo en casa.", FotoUrl = FotoGatoB, Ciudad = "Valledupar", DisponibleAdopcion = true, Activa = true, FechaPublicacionUtc = new DateTime(2026, 10, 14, 12, 0, 0, DateTimeKind.Utc) },
        new Mascota { Id = Guid.Parse("55555555-0000-0000-0000-000000000006"), RefugioId = Guid.Parse("44444444-4444-4444-4444-444444444447"), Nombre = "Nala", Especie = Especie.Perro, Raza = "Criolla", EdadMeses = 24, Descripcion = "Le encantan los juegos de olfato y la compañía.", FotoUrl = FotoPerroD, Ciudad = "Valledupar", DisponibleAdopcion = true, Activa = true, FechaPublicacionUtc = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc) },
        new Mascota { Id = Guid.Parse("55555555-0000-0000-0000-000000000007"), RefugioId = Guid.Parse("44444444-4444-4444-4444-444444444448"), Nombre = "Mora", Especie = Especie.Gato, Raza = "Doméstica", EdadMeses = 30, Descripcion = "Historia de ejemplo: adoptada en una familia local.", FotoUrl = FotoGatoC, Ciudad = "Valledupar", DisponibleAdopcion = false, Adoptada = true, Activa = true, FechaPublicacionUtc = new DateTime(2026, 8, 20, 12, 0, 0, DateTimeKind.Utc) },
        new Mascota { Id = Guid.Parse("55555555-0000-0000-0000-000000000008"), RefugioId = Guid.Parse("44444444-4444-4444-4444-444444444449"), Nombre = "Toby", Especie = Especie.Perro, Raza = "Criollo", EdadMeses = 9, Descripcion = "Tierno y curioso; disfruta conocer personas.", FotoUrl = FotoPerroA, Ciudad = "Valledupar", DisponibleAdopcion = true, Activa = true, FechaPublicacionUtc = new DateTime(2026, 10, 16, 12, 0, 0, DateTimeKind.Utc) }
    ];

    public static IReadOnlyList<ReporteMascota> ReportesMascota { get; } =
    [
        new ReporteMascota { Id = Guid.Parse("66666666-0000-0000-0000-000000000001"), UsuarioId = Guid.Parse("33333333-3333-3333-3333-333333333333"), Tipo = TipoReporte.Perdida, Especie = Especie.Perro, NombreMascota = "Toby", Raza = "Criollo", Ciudad = "Valledupar", Descripcion = "Perro café con pecho blanco, visto cerca del parque.", FotoUrl = "/images/reportes/toby-perdido.jpg", FechaEventoUtc = new DateTime(2026, 10, 18, 16, 0, 0, DateTimeKind.Utc), FechaReporteUtc = new DateTime(2026, 10, 19, 9, 0, 0, DateTimeKind.Utc), Activo = true },
        new ReporteMascota { Id = Guid.Parse("66666666-0000-0000-0000-000000000002"), UsuarioId = Guid.Parse("33333333-3333-3333-3333-333333333333"), Tipo = TipoReporte.Encontrada, Especie = Especie.Perro, NombreMascota = string.Empty, Raza = "Criollo", Ciudad = "Valledupar", Descripcion = "Perro café encontrado cerca del parque principal.", FotoUrl = "/images/reportes/perro-encontrado.jpg", FechaEventoUtc = new DateTime(2026, 10, 20, 16, 0, 0, DateTimeKind.Utc), FechaReporteUtc = new DateTime(2026, 10, 20, 18, 0, 0, DateTimeKind.Utc), Activo = true },
        new ReporteMascota { Id = Guid.Parse("66666666-0000-0000-0000-000000000003"), UsuarioId = Guid.Parse("33333333-3333-3333-3333-333333333333"), Tipo = TipoReporte.Perdida, Especie = Especie.Gato, NombreMascota = "Nube", Raza = "Doméstico", Ciudad = "Valledupar", Descripcion = "Gata gris perdida en el barrio Novalito.", FotoUrl = "/images/reportes/nube-perdida.jpg", FechaEventoUtc = new DateTime(2026, 10, 21, 8, 0, 0, DateTimeKind.Utc), FechaReporteUtc = new DateTime(2026, 10, 21, 10, 0, 0, DateTimeKind.Utc), Activo = true },
        new ReporteMascota { Id = Guid.Parse("66666666-0000-0000-0000-000000000004"), UsuarioId = Guid.Parse("33333333-3333-3333-3333-333333333333"), Tipo = TipoReporte.Encontrada, Especie = Especie.Perro, NombreMascota = "Sol", Raza = "Criolla", Ciudad = "Valledupar", Descripcion = "Caso histórico ficticio de reencuentro para la métrica de ejemplo.", FotoUrl = FotoPerroB, FechaEventoUtc = new DateTime(2026, 8, 11, 8, 0, 0, DateTimeKind.Utc), FechaReporteUtc = new DateTime(2026, 8, 11, 10, 0, 0, DateTimeKind.Utc), MascotaReunida = true, Activo = false }
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
