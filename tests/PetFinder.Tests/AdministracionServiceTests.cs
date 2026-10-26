using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PetFinder.BLL.Exceptions;
using PetFinder.BLL.Services;
using PetFinder.DAL.Interfaces;
using PetFinder.Entity.Enums;

namespace PetFinder.Tests;

public sealed class AdministracionServiceTests
{
    [Fact]
    public async Task Administrador_NoPuedeSuspenderSuPropiaCuenta()
    {
        var usuarioId = TestFixtures.UsuarioId;
        var usuarios = new Mock<IUsuarioRepository>();
        var refugios = new Mock<IRefugioRepository>(MockBehavior.Strict);
        usuarios.Setup(repo => repo.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestFixtures.Usuario(usuarioId, Rol.Administrador));
        var sut = new AdministracionService(usuarios.Object, refugios.Object, NullLogger<AdministracionService>.Instance);

        await Assert.ThrowsAsync<BusinessRuleException>(() => sut.CambiarEstadoUsuarioAsync(usuarioId, usuarioId, activo: false));
        usuarios.Verify(repo => repo.GuardarAsync(It.IsAny<PetFinder.Entity.Entities.Usuario>(), It.IsAny<CancellationToken>()), Times.Never);
        refugios.VerifyNoOtherCalls();
    }
}
