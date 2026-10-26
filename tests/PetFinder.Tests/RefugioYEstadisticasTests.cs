using Moq;
using PetFinder.BLL.Services;
using PetFinder.DAL.Interfaces;
using PetFinder.Entity.Entities;
using PetFinder.Entity.Enums;

namespace PetFinder.Tests;

public sealed class RefugioYEstadisticasTests
{
    [Fact]
    public async Task RefugioService_ExcluyeCoordenadasInvalidasYCuentaMascotasPorEspecie()
    {
        var valido = Refugio(Guid.Parse("00000000-0000-0000-0000-000000000001"), 10.46m, -73.25m);
        var latitudInvalida = Refugio(Guid.Parse("00000000-0000-0000-0000-000000000002"), 91m, -73.25m);
        var sinUbicacion = Refugio(Guid.Parse("00000000-0000-0000-0000-000000000003"), null, null);
        var inactivo = Refugio(Guid.Parse("00000000-0000-0000-0000-000000000004"), 10.45m, -73.24m);
        inactivo.Activo = false;
        var otraCiudad = Refugio(Guid.Parse("00000000-0000-0000-0000-000000000005"), 10.47m, -73.26m);
        otraCiudad.Ciudad = "Riohacha";
        var refugios = new Mock<IRefugioRepository>(MockBehavior.Strict);
        var mascotas = new Mock<IMascotaRepository>(MockBehavior.Strict);
        refugios.Setup(repo => repo.ObtenerTodosAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { valido, latitudInvalida, sinUbicacion, inactivo, otraCiudad });
        mascotas.Setup(repo => repo.ObtenerDisponiblesAdopcionAsync(null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                TestFixtures.Mascota(refugioId: valido.Id),
                new Mascota { Id = Guid.NewGuid(), RefugioId = valido.Id, Especie = Especie.Gato, DisponibleAdopcion = true },
                TestFixtures.Mascota(refugioId: latitudInvalida.Id)
            });

        var result = await new RefugioService(refugios.Object, mascotas.Object).ObtenerMapaAsync();

        var refugioResultante = Assert.Single(result);
        Assert.Equal(valido.Id, refugioResultante.Id);
        Assert.Equal(2, refugioResultante.MascotasDisponibles);
        Assert.Equal(new[] { Especie.Perro, Especie.Gato }, refugioResultante.EspeciesDisponibles.OrderBy(especie => especie));
        refugios.VerifyAll();
        mascotas.VerifyAll();
    }

    [Fact]
    public async Task RefugioService_OrdenaPorDistanciaHaversineDesdeLaUbicacionDelUsuario()
    {
        var cercano = Refugio(Guid.Parse("00000000-0000-0000-0000-000000000011"), 10.4630m, -73.2530m);
        var lejano = Refugio(Guid.Parse("00000000-0000-0000-0000-000000000012"), 10.4740m, -73.2430m);
        var refugios = new Mock<IRefugioRepository>(MockBehavior.Strict);
        var mascotas = new Mock<IMascotaRepository>(MockBehavior.Strict);
        refugios.Setup(repo => repo.ObtenerTodosAsync(true, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { lejano, cercano });
        mascotas.Setup(repo => repo.ObtenerDisponiblesAdopcionAsync(null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Mascota>());

        var result = await new RefugioService(refugios.Object, mascotas.Object).ObtenerMapaAsync(10.4630m, -73.2530m);

        Assert.Equal(new[] { cercano.Id, lejano.Id }, result.Select(refugio => refugio.Id));
        Assert.True(RefugioService.CalcularDistanciaKm(10.4630m, -73.2530m, lejano.Latitud!.Value, lejano.Longitud!.Value) > 0);
    }

    [Fact]
    public async Task RefugioService_RechazaOrigenIncompletoAntesDeConsultarRepositorios()
    {
        var refugios = new Mock<IRefugioRepository>(MockBehavior.Strict);
        var mascotas = new Mock<IMascotaRepository>(MockBehavior.Strict);
        var servicio = new RefugioService(refugios.Object, mascotas.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => servicio.ObtenerMapaAsync(10m, null));
        refugios.VerifyNoOtherCalls();
        mascotas.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EstadisticasService_CuentaSoloEstadosExplicitosDelDominio()
    {
        var refugio = Refugio(Guid.NewGuid(), 10m, -73m);
        var adoptada = new Mascota { Id = Guid.NewGuid(), Adoptada = true };
        var disponible = new Mascota { Id = Guid.NewGuid(), DisponibleAdopcion = true };
        var reunida = new ReporteMascota { Id = Guid.NewGuid(), MascotaReunida = true, Activo = false };
        var avisoActivo = new ReporteMascota { Id = Guid.NewGuid(), Activo = true };
        var refugios = new Mock<IRefugioRepository>(MockBehavior.Strict);
        var mascotas = new Mock<IMascotaRepository>(MockBehavior.Strict);
        var reportes = new Mock<IReporteRepository>(MockBehavior.Strict);
        refugios.Setup(repo => repo.ObtenerTodosAsync(true, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { refugio });
        mascotas.Setup(repo => repo.ObtenerTodasAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { adoptada, disponible });
        reportes.Setup(repo => repo.ObtenerTodosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { reunida, avisoActivo });

        var result = await new EstadisticasService(refugios.Object, mascotas.Object, reportes.Object).ObtenerAsync();

        Assert.Equal(1, result.MascotasReunidas);
        Assert.Equal(1, result.RefugiosAliados);
        Assert.Equal(1, result.Adopciones);
        Assert.Equal(1, result.ReportesActivos);
        refugios.VerifyAll();
        mascotas.VerifyAll();
        reportes.VerifyAll();
    }

    private static Refugio Refugio(Guid id, decimal? latitud, decimal? longitud) => new()
    {
        Id = id,
        UsuarioId = Guid.NewGuid(),
        Nombre = $"Refugio {id:N}",
        Descripcion = "Datos ficticios de prueba.",
        Ciudad = "Valledupar",
        Direccion = "Dirección ficticia",
        Horario = "Horario de prueba",
        FotoUrl = "/images/refugio.jpg",
        Latitud = latitud,
        Longitud = longitud,
        Telefono = "+57 300 000 0000",
        CorreoElectronico = $"{id:N}@test.local",
        Activo = true
    };
}
