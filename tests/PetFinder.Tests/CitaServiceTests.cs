using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PetFinder.BLL.Dtos;
using PetFinder.BLL.Exceptions;
using PetFinder.BLL.Services;
using PetFinder.BLL.Validators;
using PetFinder.DAL.Interfaces;
using PetFinder.Entity.Entities;
using PetFinder.Entity.Enums;

namespace PetFinder.Tests;

public sealed class CitaServiceTests
{
    private static readonly DateTime Inicio = TestFixtures.AhoraUtc.AddDays(2);

    [Fact]
    public async Task SolicitarAsync_RechazaCruceYNoPersisteCita()
    {
        var harness = CrearServicioReserva();
        harness.Citas.Setup(repo => repo.ObtenerCrucesDeHorarioAsync(
                TestFixtures.RefugioId,
                Inicio,
                Inicio.AddMinutes(30),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new Cita { Estado = EstadoCita.Pendiente } });

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => harness.Sut.SolicitarAsync(Solicitud()));
        Assert.Contains("cruza", exception.Message, StringComparison.OrdinalIgnoreCase);
        harness.Citas.Verify(repo => repo.GuardarAsync(It.IsAny<Cita>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SolicitarAsync_CreaCitaPendienteCuandoElIntervaloEstaLibre()
    {
        var harness = CrearServicioReserva();
        Cita? guardada = null;
        harness.Citas.Setup(repo => repo.ObtenerCrucesDeHorarioAsync(
                TestFixtures.RefugioId,
                Inicio,
                Inicio.AddMinutes(45),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Cita>());
        harness.Citas.Setup(repo => repo.GuardarAsync(It.IsAny<Cita>(), It.IsAny<CancellationToken>()))
            .Callback<Cita, CancellationToken>((cita, _) => guardada = cita)
            .Returns(Task.CompletedTask);

        var result = await harness.Sut.SolicitarAsync(Solicitud(duracion: 45));

        Assert.NotNull(guardada);
        Assert.Equal(EstadoCita.Pendiente, result.Estado);
        Assert.Equal(Inicio, result.FechaInicioUtc);
        Assert.Equal(Inicio.AddMinutes(45), result.FechaFinUtc);
        Assert.Equal("Visita a Luna", result.Notas);
        Assert.Equal(result.Id, guardada.Id);
    }

    [Fact]
    public async Task SolicitarAsync_RechazaFechaPasada()
    {
        var harness = CrearServicioReserva();
        await Assert.ThrowsAsync<BusinessRuleException>(() => harness.Sut.SolicitarAsync(Solicitud(fecha: TestFixtures.AhoraUtc.AddMinutes(-1))));
        harness.Citas.Verify(repo => repo.ObtenerCrucesDeHorarioAsync(
            It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SolicitarAsync_RechazaFechaNoUtc()
    {
        var harness = CrearServicioReserva();
        var fechaLocal = DateTime.SpecifyKind(Inicio, DateTimeKind.Local);
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() => harness.Sut.SolicitarAsync(Solicitud(fecha: fechaLocal)));
        Assert.Contains("UTC", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SolicitarAsync_RechazaMascotaDeOtroRefugio()
    {
        var harness = CrearServicioReserva(mascota: TestFixtures.Mascota(refugioId: Guid.NewGuid()));
        await Assert.ThrowsAsync<BusinessRuleException>(() => harness.Sut.SolicitarAsync(Solicitud()));
        harness.Citas.Verify(repo => repo.ObtenerCrucesDeHorarioAsync(
            It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmarAsync_SoloPermiteAlRefugioPropietario()
    {
        var cita = CitaSeed();
        var harness = CrearServicioTransiciones(cita);
        harness.Usuarios.Setup(repo => repo.ObtenerPorIdAsync(TestFixtures.UsuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestFixtures.Usuario(rol: Rol.Adoptante));

        await Assert.ThrowsAsync<BusinessRuleException>(() => harness.Sut.ConfirmarAsync(cita.Id, TestFixtures.UsuarioId));
        Assert.Equal(EstadoCita.Pendiente, cita.Estado);
        harness.Citas.Verify(repo => repo.GuardarAsync(It.IsAny<Cita>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmarAsync_RefugioPropietarioCambiaPendienteAConfirmada()
    {
        var cita = CitaSeed();
        var refugioUsuarioId = Guid.NewGuid();
        var harness = CrearServicioTransiciones(cita);
        harness.Usuarios.Setup(repo => repo.ObtenerPorIdAsync(refugioUsuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestFixtures.Usuario(refugioUsuarioId, Rol.Refugio));
        harness.Refugios.Setup(repo => repo.ObtenerTodosAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { TestFixtures.Refugio(TestFixtures.RefugioId, refugioUsuarioId) });
        harness.Citas.Setup(repo => repo.GuardarAsync(cita, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await harness.Sut.ConfirmarAsync(cita.Id, refugioUsuarioId);
        Assert.Equal(EstadoCita.Confirmada, result.Estado);
    }

    [Fact]
    public async Task ConfirmarAsync_RechazaUnaCitaQueYaNoEstaPendiente()
    {
        var cita = CitaSeed(EstadoCita.Cancelada);
        var adminId = Guid.NewGuid();
        var harness = CrearServicioTransiciones(cita);
        harness.Usuarios.Setup(repo => repo.ObtenerPorIdAsync(adminId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestFixtures.Usuario(adminId, Rol.Administrador));
        await Assert.ThrowsAsync<BusinessRuleException>(() => harness.Sut.ConfirmarAsync(cita.Id, adminId));
    }

    [Fact]
    public async Task CancelarAsync_RechazaAdoptanteQueNoEsPropietario()
    {
        var cita = CitaSeed(EstadoCita.Confirmada);
        var otroAdoptanteId = Guid.NewGuid();
        var harness = CrearServicioTransiciones(cita);
        harness.Usuarios.Setup(repo => repo.ObtenerPorIdAsync(otroAdoptanteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestFixtures.Usuario(otroAdoptanteId, Rol.Adoptante));
        await Assert.ThrowsAsync<BusinessRuleException>(() => harness.Sut.CancelarAsync(cita.Id, otroAdoptanteId));
        Assert.Equal(EstadoCita.Confirmada, cita.Estado);
    }

    [Fact]
    public async Task ObtenerPorRefugioAsync_RechazaUnaCuentaNoRefugio()
    {
        var harness = CrearServicioTransiciones(CitaSeed());
        harness.Usuarios.Setup(repo => repo.ObtenerPorIdAsync(TestFixtures.UsuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestFixtures.Usuario(rol: Rol.Adoptante));
        await Assert.ThrowsAsync<BusinessRuleException>(() => harness.Sut.ObtenerPorRefugioAsync(TestFixtures.UsuarioId));
        harness.Citas.Verify(repo => repo.ObtenerPorRefugioYFechaAsync(
            It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static SolicitarCitaDto Solicitud(DateTime? fecha = null, int duracion = 30) => new()
    {
        UsuarioId = TestFixtures.UsuarioId,
        RefugioId = TestFixtures.RefugioId,
        MascotaId = TestFixtures.MascotaId,
        FechaInicioUtc = fecha ?? Inicio,
        DuracionMinutos = duracion,
        Notas = " Visita a Luna "
    };

    private static Cita CitaSeed(EstadoCita estado = EstadoCita.Pendiente) => new()
    {
        Id = TestFixtures.CitaId,
        UsuarioId = TestFixtures.UsuarioId,
        RefugioId = TestFixtures.RefugioId,
        MascotaId = TestFixtures.MascotaId,
        FechaInicioUtc = Inicio,
        FechaFinUtc = Inicio.AddMinutes(30),
        FechaCreacionUtc = TestFixtures.AhoraUtc,
        Estado = estado,
        Notas = "Visita"
    };

    private static Harness CrearServicioReserva(PetFinder.Entity.Entities.Mascota? mascota = null)
    {
        var citaRepo = new Mock<ICitaRepository>();
        var usuarioRepo = new Mock<IUsuarioRepository>();
        var refugioRepo = new Mock<IRefugioRepository>();
        var mascotaRepo = new Mock<IMascotaRepository>();
        usuarioRepo.Setup(repo => repo.ObtenerPorIdAsync(TestFixtures.UsuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestFixtures.Usuario());
        refugioRepo.Setup(repo => repo.ObtenerPorIdAsync(TestFixtures.RefugioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestFixtures.Refugio());
        mascotaRepo.Setup(repo => repo.ObtenerPorIdAsync(TestFixtures.MascotaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mascota ?? TestFixtures.Mascota());
        var sut = new CitaService(
            citaRepo.Object,
            usuarioRepo.Object,
            refugioRepo.Object,
            mascotaRepo.Object,
            new SolicitarCitaValidator(),
            new FrozenTimeProvider(new DateTimeOffset(TestFixtures.AhoraUtc)),
            NullLogger<CitaService>.Instance);
        return new Harness(sut, citaRepo, usuarioRepo, refugioRepo);
    }

    private static Harness CrearServicioTransiciones(Cita cita)
    {
        var citaRepo = new Mock<ICitaRepository>();
        citaRepo.Setup(repo => repo.ObtenerPorIdAsync(cita.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cita);
        var usuarios = new Mock<IUsuarioRepository>();
        var refugios = new Mock<IRefugioRepository>();
        var mascotas = new Mock<IMascotaRepository>();
        var sut = new CitaService(
            citaRepo.Object,
            usuarios.Object,
            refugios.Object,
            mascotas.Object,
            new SolicitarCitaValidator(),
            new FrozenTimeProvider(new DateTimeOffset(TestFixtures.AhoraUtc)),
            NullLogger<CitaService>.Instance);
        return new Harness(sut, citaRepo, usuarios, refugios);
    }

    private sealed record Harness(
        CitaService Sut,
        Mock<ICitaRepository> Citas,
        Mock<IUsuarioRepository> Usuarios,
        Mock<IRefugioRepository> Refugios);
}
