using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PetFinder.BLL.Dtos;
using PetFinder.BLL.Exceptions;
using PetFinder.BLL.Services;
using PetFinder.DAL.Interfaces;
using PetFinder.Entity.Enums;

namespace PetFinder.Tests;

public sealed class GestionRefugioServiceTests
{
    [Fact]
    public async Task ObtenerPanelAsync_SoloIncluyeMascotasDelRefugioPropio()
    {
        var actor = TestFixtures.Usuario(TestFixtures.UsuarioId, Rol.Refugio);
        var propio = TestFixtures.Refugio(usuarioId: actor.Id);
        var otraMascota = TestFixtures.Mascota(Guid.NewGuid(), Guid.NewGuid());
        var propia = TestFixtures.Mascota(refugioId: propio.Id);
        var users = new Mock<IUsuarioRepository>();
        var refuges = new Mock<IRefugioRepository>();
        var pets = new Mock<IMascotaRepository>();
        users.Setup(repo => repo.ObtenerPorIdAsync(actor.Id, It.IsAny<CancellationToken>())).ReturnsAsync(actor);
        refuges.Setup(repo => repo.ObtenerTodosAsync(false, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { propio });
        pets.Setup(repo => repo.ObtenerTodasAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { otraMascota, propia });
        var sut = new GestionRefugioService(users.Object, refuges.Object, pets.Object, NullLogger<GestionRefugioService>.Instance);

        var panel = await sut.ObtenerPanelAsync(actor.Id);
        Assert.Equal(propio.Id, panel.Refugio.Id);
        Assert.Single(panel.Mascotas);
        Assert.Equal(propia.Id, panel.Mascotas.Single().Id);
    }

    [Fact]
    public async Task GuardarMascotaAsync_CreaPublicacionConPropietarioDerivadoDelRefugio()
    {
        var actor = TestFixtures.Usuario(TestFixtures.UsuarioId, Rol.Refugio);
        var refugio = TestFixtures.Refugio(usuarioId: actor.Id);
        PetFinder.Entity.Entities.Mascota? guardada = null;
        var users = new Mock<IUsuarioRepository>();
        var refuges = new Mock<IRefugioRepository>();
        var pets = new Mock<IMascotaRepository>();
        users.Setup(repo => repo.ObtenerPorIdAsync(actor.Id, It.IsAny<CancellationToken>())).ReturnsAsync(actor);
        refuges.Setup(repo => repo.ObtenerTodosAsync(false, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { refugio });
        pets.Setup(repo => repo.GuardarAsync(It.IsAny<PetFinder.Entity.Entities.Mascota>(), It.IsAny<CancellationToken>()))
            .Callback<PetFinder.Entity.Entities.Mascota, CancellationToken>((mascota, _) => guardada = mascota)
            .Returns(Task.CompletedTask);
        var sut = new GestionRefugioService(users.Object, refuges.Object, pets.Object, NullLogger<GestionRefugioService>.Instance);

        var result = await sut.GuardarMascotaAsync(actor.Id, new GuardarMascotaRefugioDto(
            null, "  Nube ", Especie.Gato, "Criolla", 12, "Gatita tranquila.", "/nube.jpg", "Valledupar", true));

        Assert.NotNull(guardada);
        Assert.Equal(refugio.Id, result.RefugioId);
        Assert.Equal("Nube", result.Nombre);
        Assert.True(result.Activa);
    }

    [Fact]
    public async Task GuardarMascotaAsync_RechazaEdicionDePublicacionAjena()
    {
        var actor = TestFixtures.Usuario(TestFixtures.UsuarioId, Rol.Refugio);
        var refugio = TestFixtures.Refugio(usuarioId: actor.Id);
        var mascotaAjena = TestFixtures.Mascota(Guid.NewGuid(), Guid.NewGuid());
        var users = new Mock<IUsuarioRepository>();
        var refuges = new Mock<IRefugioRepository>();
        var pets = new Mock<IMascotaRepository>();
        users.Setup(repo => repo.ObtenerPorIdAsync(actor.Id, It.IsAny<CancellationToken>())).ReturnsAsync(actor);
        refuges.Setup(repo => repo.ObtenerTodosAsync(false, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { refugio });
        pets.Setup(repo => repo.ObtenerPorIdAsync(mascotaAjena.Id, It.IsAny<CancellationToken>())).ReturnsAsync(mascotaAjena);
        var sut = new GestionRefugioService(users.Object, refuges.Object, pets.Object, NullLogger<GestionRefugioService>.Instance);

        await Assert.ThrowsAsync<BusinessRuleException>(() => sut.GuardarMascotaAsync(actor.Id, new GuardarMascotaRefugioDto(
            mascotaAjena.Id, "Nube", Especie.Gato, "Criolla", 12, "Gatita tranquila.", "/nube.jpg", "Valledupar", true)));
        pets.Verify(repo => repo.GuardarAsync(It.IsAny<PetFinder.Entity.Entities.Mascota>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
