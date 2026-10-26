using FluentValidation;
using Moq;
using PetFinder.BLL.Dtos;
using PetFinder.BLL.Exceptions;
using PetFinder.BLL.Services;
using PetFinder.BLL.Validators;
using PetFinder.DAL.Interfaces;
using PetFinder.Entity.Enums;

namespace PetFinder.Tests;

public sealed class AdopcionYCoincidenciaTests
{
    [Fact]
    public async Task Coincidencias_FiltraCandidatosAntesDeCompararYOrdenaPorPorcentaje()
    {
        var origenId = Guid.NewGuid();
        var primero = TestFixtures.Reporte(Guid.NewGuid(), TipoReporte.Encontrada, "/images/a.jpg");
        var segundo = TestFixtures.Reporte(Guid.NewGuid(), TipoReporte.Encontrada, "/images/b.jpg");
        var mismoTipo = TestFixtures.Reporte(Guid.NewGuid(), TipoReporte.Perdida, "/images/c.jpg");
        var sinFoto = TestFixtures.Reporte(Guid.NewGuid(), TipoReporte.Encontrada, " ");
        var inactivo = TestFixtures.Reporte(Guid.NewGuid(), TipoReporte.Encontrada, "/images/d.jpg", activo: false);
        var origen = TestFixtures.Reporte(origenId, TipoReporte.Perdida, "/images/origen.jpg");
        var reportes = new Mock<IReporteRepository>(MockBehavior.Strict);
        var imagen = new Mock<IImageSimilarityService>(MockBehavior.Strict);
        reportes.Setup(repo => repo.ObtenerPorIdAsync(origenId, It.IsAny<CancellationToken>())).ReturnsAsync(origen);
        reportes.Setup(repo => repo.ObtenerCandidatosAsync(
                Especie.Perro,
                "Valledupar",
                origen.FechaReporteUtc.AddDays(-21),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { primero, segundo, mismoTipo, sinFoto, inactivo, origen });
        imagen.Setup(service => service.CalcularPorcentajeAsync(origen.FotoUrl, primero.FotoUrl, It.IsAny<CancellationToken>())).ReturnsAsync(150m);
        imagen.Setup(service => service.CalcularPorcentajeAsync(origen.FotoUrl, segundo.FotoUrl, It.IsAny<CancellationToken>())).ReturnsAsync(72m);

        var sut = new CoincidenciaService(reportes.Object, imagen.Object, new BuscarCoincidenciasValidator());
        var result = await sut.BuscarParaReporteAsync(origenId, diasRecientes: 21);

        Assert.Equal(2, result.Count);
        Assert.Equal(primero.Id, result[0].Reporte.Id);
        Assert.Equal(100m, result[0].PorcentajeSimilitud);
        Assert.Equal(segundo.Id, result[1].Reporte.Id);
        Assert.Equal(72m, result[1].PorcentajeSimilitud);
        imagen.Verify(service => service.CalcularPorcentajeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        reportes.VerifyAll();
    }

    [Fact]
    public async Task Coincidencias_LanzaNotFoundSiNoExisteReporteOrigen()
    {
        var id = Guid.NewGuid();
        var repo = new Mock<IReporteRepository>();
        repo.Setup(item => item.ObtenerPorIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((PetFinder.Entity.Entities.ReporteMascota?)null);
        var sut = new CoincidenciaService(repo.Object, Mock.Of<IImageSimilarityService>(), new BuscarCoincidenciasValidator());
        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.BuscarParaReporteAsync(id));
    }

    [Fact]
    public async Task Catalogo_ExcluyeMascotasDeRefugiosInactivos()
    {
        var refugioActivo = TestFixtures.Refugio();
        var refugioInactivo = TestFixtures.Refugio(Guid.NewGuid(), Guid.NewGuid(), activo: false);
        var mascotaActiva = TestFixtures.Mascota(refugioId: refugioActivo.Id);
        var mascotaDeRefugioInactivo = TestFixtures.Mascota(Guid.NewGuid(), refugioInactivo.Id);
        var mascotas = new Mock<IMascotaRepository>(MockBehavior.Strict);
        var refugios = new Mock<IRefugioRepository>(MockBehavior.Strict);
        mascotas.Setup(repo => repo.ObtenerDisponiblesAdopcionAsync(refugioActivo.Id, Especie.Perro, 5, 24, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { mascotaActiva, mascotaDeRefugioInactivo });
        refugios.Setup(repo => repo.ObtenerTodosAsync(true, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { refugioActivo });

        var sut = new AdopcionService(mascotas.Object, refugios.Object);
        var result = await sut.ObtenerCatalogoAsync(refugioActivo.Id, Especie.Perro, 5, 24);

        Assert.Single(result);
        Assert.Equal(mascotaActiva.Id, result.Single().Id);
        mascotas.VerifyAll();
        refugios.VerifyAll();
    }

    [Fact]
    public async Task Catalogo_RechazaRangoDeEdadInvertidoAntesDeConsultarRepositorios()
    {
        var mascotas = new Mock<IMascotaRepository>(MockBehavior.Strict);
        var refugios = new Mock<IRefugioRepository>(MockBehavior.Strict);
        var sut = new AdopcionService(mascotas.Object, refugios.Object);
        await Assert.ThrowsAsync<BusinessRuleException>(() => sut.ObtenerCatalogoAsync(edadMinimaMeses: 24, edadMaximaMeses: 3));
        mascotas.VerifyNoOtherCalls();
        refugios.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Detalle_RechazaMascotaSiSuRefugioEstaInactivo()
    {
        var mascota = TestFixtures.Mascota();
        var repoMascotas = new Mock<IMascotaRepository>();
        var repoRefugios = new Mock<IRefugioRepository>();
        repoMascotas.Setup(repo => repo.ObtenerPorIdAsync(mascota.Id, It.IsAny<CancellationToken>())).ReturnsAsync(mascota);
        repoRefugios.Setup(repo => repo.ObtenerPorIdAsync(mascota.RefugioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestFixtures.Refugio(activo: false));
        var sut = new AdopcionService(repoMascotas.Object, repoRefugios.Object);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.ObtenerDetalleAsync(mascota.Id));
    }
}
