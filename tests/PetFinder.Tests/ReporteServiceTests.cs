using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PetFinder.BLL.Dtos;
using PetFinder.BLL.Exceptions;
using PetFinder.BLL.Services;
using PetFinder.BLL.Validators;
using PetFinder.DAL.Interfaces;
using PetFinder.Entity.Enums;

namespace PetFinder.Tests;

public sealed class ReporteServiceTests
{
    [Fact]
    public async Task CrearAsync_NormalizaCamposYUsaRelojInyectado()
    {
        PetFinder.Entity.Entities.ReporteMascota? guardado = null;
        var reportes = new Mock<IReporteRepository>();
        var usuarios = new Mock<IUsuarioRepository>();
        usuarios.Setup(repo => repo.ObtenerPorIdAsync(TestFixtures.UsuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestFixtures.Usuario());
        reportes.Setup(repo => repo.GuardarAsync(It.IsAny<PetFinder.Entity.Entities.ReporteMascota>(), It.IsAny<CancellationToken>()))
            .Callback<PetFinder.Entity.Entities.ReporteMascota, CancellationToken>((reporte, _) => guardado = reporte)
            .Returns(Task.CompletedTask);
        var sut = new ReporteService(
            reportes.Object,
            usuarios.Object,
            new CrearReporteMascotaValidator(),
            new FrozenTimeProvider(new DateTimeOffset(TestFixtures.AhoraUtc)),
            NullLogger<ReporteService>.Instance);

        var result = await sut.CrearAsync(TestFixtures.UsuarioId, ReporteDto());
        Assert.NotNull(guardado);
        Assert.Equal("Valledupar", result.Ciudad);
        Assert.Equal("Toby", result.NombreMascota);
        Assert.Equal(TestFixtures.AhoraUtc, result.FechaReporteUtc);
        Assert.Equal(DateTimeKind.Utc, result.FechaEventoUtc.Kind);
        Assert.Equal(result.Id, guardado.Id);
    }

    [Fact]
    public async Task CrearAsync_RechazaFechaDeEventoFutura()
    {
        var reportes = new Mock<IReporteRepository>();
        var usuarios = new Mock<IUsuarioRepository>();
        usuarios.Setup(repo => repo.ObtenerPorIdAsync(TestFixtures.UsuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestFixtures.Usuario());
        var sut = new ReporteService(
            reportes.Object,
            usuarios.Object,
            new CrearReporteMascotaValidator(),
            new FrozenTimeProvider(new DateTimeOffset(TestFixtures.AhoraUtc)),
            NullLogger<ReporteService>.Instance);

        await Assert.ThrowsAsync<BusinessRuleException>(() => sut.CrearAsync(TestFixtures.UsuarioId, ReporteDto(TestFixtures.AhoraUtc.AddMinutes(1))));
        reportes.Verify(repo => repo.GuardarAsync(It.IsAny<PetFinder.Entity.Entities.ReporteMascota>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CrearAsync_RechazaReferenciaDeImagenConEsquemaPeligroso()
    {
        var reportes = new Mock<IReporteRepository>(MockBehavior.Strict);
        var usuarios = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        var sut = new ReporteService(
            reportes.Object,
            usuarios.Object,
            new CrearReporteMascotaValidator(),
            new FrozenTimeProvider(new DateTimeOffset(TestFixtures.AhoraUtc)),
            NullLogger<ReporteService>.Instance);
        var dto = ReporteDto();
        dto = new CrearReporteMascotaDto
        {
            Tipo = dto.Tipo,
            Especie = dto.Especie,
            NombreMascota = dto.NombreMascota,
            Raza = dto.Raza,
            Ciudad = dto.Ciudad,
            Descripcion = dto.Descripcion,
            FotoUrl = "javascript:alert(1)",
            FechaEventoUtc = dto.FechaEventoUtc
        };

        await Assert.ThrowsAsync<ValidationException>(() => sut.CrearAsync(TestFixtures.UsuarioId, dto));
        reportes.VerifyNoOtherCalls();
        usuarios.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CrearAsync_RechazaUsuarioInexistente()
    {
        var users = new Mock<IUsuarioRepository>();
        users.Setup(repo => repo.ObtenerPorIdAsync(TestFixtures.UsuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PetFinder.Entity.Entities.Usuario?)null);
        var sut = new ReporteService(
            Mock.Of<IReporteRepository>(),
            users.Object,
            new CrearReporteMascotaValidator(),
            new FrozenTimeProvider(new DateTimeOffset(TestFixtures.AhoraUtc)),
            NullLogger<ReporteService>.Instance);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.CrearAsync(TestFixtures.UsuarioId, ReporteDto()));
    }

    private static CrearReporteMascotaDto ReporteDto(DateTime? fecha = null) => new()
    {
        Tipo = TipoReporte.Perdida,
        Especie = Especie.Perro,
        NombreMascota = " Toby ",
        Raza = " Criollo ",
        Ciudad = " Valledupar ",
        Descripcion = " Perro café con pecho blanco. ",
        FotoUrl = "/uploads/reports/toby.jpg",
        FechaEventoUtc = fecha ?? TestFixtures.AhoraUtc.AddDays(-2)
    };
}
