using Microsoft.Extensions.Logging.Abstractions;
using PetFinder.Entity.Entities;
using PetFinder.Entity.Enums;

namespace PetFinder.Tests;

internal static class TestFixtures
{
    public static readonly DateTime AhoraUtc = new(2035, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    public static readonly Guid UsuarioId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid RefugioId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid MascotaId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    public static readonly Guid CitaId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    public static Usuario Usuario(Guid? id = null, Rol rol = Rol.Adoptante, bool activo = true, string correo = "ana@example.com") => new()
    {
        Id = id ?? UsuarioId,
        NombreCompleto = "Ana Pérez",
        CorreoElectronico = correo,
        PasswordHash = string.Empty,
        Rol = rol,
        Activo = activo,
        FechaRegistroUtc = AhoraUtc.AddDays(-10)
    };

    public static Refugio Refugio(Guid? id = null, Guid? usuarioId = null, bool activo = true) => new()
    {
        Id = id ?? RefugioId,
        UsuarioId = usuarioId ?? UsuarioId,
        Nombre = "Huellitas",
        Descripcion = "Refugio aliado",
        Ciudad = "Valledupar",
        Direccion = "Calle 1",
        CorreoElectronico = "hola@huellitas.test",
        Activo = activo,
        FechaRegistroUtc = AhoraUtc.AddDays(-20)
    };

    public static Mascota Mascota(Guid? id = null, Guid? refugioId = null, bool activa = true, bool disponible = true) => new()
    {
        Id = id ?? MascotaId,
        RefugioId = refugioId ?? RefugioId,
        Nombre = "Luna",
        Especie = Especie.Perro,
        Raza = "Criolla",
        EdadMeses = 18,
        Descripcion = "Cariñosa y sociable.",
        FotoUrl = "/images/luna.jpg",
        Ciudad = "Valledupar",
        DisponibleAdopcion = disponible,
        Activa = activa,
        FechaPublicacionUtc = AhoraUtc.AddDays(-2)
    };

    public static ReporteMascota Reporte(Guid id, TipoReporte tipo, string foto = "/images/pet.jpg", bool activo = true) => new()
    {
        Id = id,
        UsuarioId = UsuarioId,
        Tipo = tipo,
        Especie = Especie.Perro,
        NombreMascota = "Toby",
        Raza = "Criollo",
        Ciudad = "Valledupar",
        Descripcion = "Mancha blanca en el pecho.",
        FotoUrl = foto,
        FechaEventoUtc = AhoraUtc.AddDays(-2),
        FechaReporteUtc = AhoraUtc.AddDays(-1),
        Activo = activo
    };
}

internal sealed class FrozenTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}
