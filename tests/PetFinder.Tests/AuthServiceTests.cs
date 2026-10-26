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

public sealed class AuthServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarios = new();
    private readonly AuthService _sut;

    public AuthServiceTests() => _sut = new AuthService(
        _usuarios.Object,
        new RegistroUsuarioValidator(),
        new InicioSesionValidator(),
        NullLogger<AuthService>.Instance);

    [Fact]
    public async Task RegistrarAsync_GuardaHashPBKDF2YDevuelveSoloSesionSegura()
    {
        PetFinder.Entity.Entities.Usuario? guardado = null;
        _usuarios.Setup(repo => repo.ObtenerPorCorreoAsync("ana@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((PetFinder.Entity.Entities.Usuario?)null);
        _usuarios.Setup(repo => repo.GuardarAsync(It.IsAny<PetFinder.Entity.Entities.Usuario>(), It.IsAny<CancellationToken>()))
            .Callback<PetFinder.Entity.Entities.Usuario, CancellationToken>((usuario, _) => guardado = usuario)
            .Returns(Task.CompletedTask);

        var sesion = await _sut.RegistrarAsync(new RegistroUsuarioDto
        {
            NombreCompleto = " Ana Pérez ",
            CorreoElectronico = " ana@example.com ",
            Contrasena = "Secreto123"
        });

        Assert.NotNull(guardado);
        Assert.StartsWith("PBKDF2-SHA256$210000$", guardado.PasswordHash);
        Assert.NotEqual("Secreto123", guardado.PasswordHash);
        Assert.Equal("Ana Pérez", guardado.NombreCompleto);
        Assert.Equal(Rol.Adoptante, guardado.Rol);
        Assert.Equal(guardado.Id, sesion.Id);
        Assert.Equal("ana@example.com", sesion.CorreoElectronico);
        Assert.DoesNotContain(typeof(SesionUsuarioDto).GetProperties(), property => property.Name.Contains("Password", StringComparison.OrdinalIgnoreCase));

        _usuarios.Setup(repo => repo.ObtenerPorCorreoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(guardado);
        var autenticada = await _sut.IniciarSesionAsync(new InicioSesionDto
        {
            CorreoElectronico = "ANA@example.com",
            Contrasena = "Secreto123"
        });
        Assert.Equal(sesion.Id, autenticada.Id);
    }

    [Fact]
    public async Task RegistrarAsync_RechazaCorreoDuplicadoSinGuardar()
    {
        _usuarios.Setup(repo => repo.ObtenerPorCorreoAsync("ana@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestFixtures.Usuario());

        await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.RegistrarAsync(new RegistroUsuarioDto
        {
            NombreCompleto = "Ana Pérez",
            CorreoElectronico = "ana@example.com",
            Contrasena = "Secreto123"
        }));
        _usuarios.Verify(repo => repo.GuardarAsync(It.IsAny<PetFinder.Entity.Entities.Usuario>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("secreto1", "La contraseña debe contener al menos una mayúscula.")]
    [InlineData("SECRETO1", "La contraseña debe contener al menos una minúscula.")]
    [InlineData("SecretoXX", "La contraseña debe contener al menos un número.")]
    public async Task RegistrarAsync_RechazaContrasenaQueNoCumplePolitica(string password, string _)
    {
        await Assert.ThrowsAsync<ValidationException>(() => _sut.RegistrarAsync(new RegistroUsuarioDto
        {
            NombreCompleto = "Ana Pérez",
            CorreoElectronico = "ana@example.com",
            Contrasena = password
        }));
        _usuarios.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task IniciarSesionAsync_RechazaUsuarioInactivoConMensajeGenerico()
    {
        var usuario = TestFixtures.Usuario(activo: false);
        usuario.PasswordHash = "PBKDF2-SHA256$210000$%%%$%%%";
        _usuarios.Setup(repo => repo.ObtenerPorCorreoAsync(usuario.CorreoElectronico, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.IniciarSesionAsync(new InicioSesionDto
        {
            CorreoElectronico = usuario.CorreoElectronico,
            Contrasena = "Secreto123"
        }));
        Assert.Equal("El correo electrónico o la contraseña no son válidos.", error.Message);
    }
}
