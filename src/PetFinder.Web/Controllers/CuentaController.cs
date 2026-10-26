using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetFinder.BLL.Dtos;
using PetFinder.BLL.Exceptions;
using PetFinder.BLL.Services;
using PetFinder.Entity.Enums;
using PetFinder.Web.ViewModels;

namespace PetFinder.Web.Controllers;

/// <summary>Controla el registro, autenticación y cierre de sesión con cookies.</summary>
public sealed class CuentaController(AuthService authService, IWebHostEnvironment environment, ILogger<CuentaController> logger) : Controller
{
    /// <summary>Muestra el formulario de inicio de sesión.</summary>
    [AllowAnonymous, HttpGet]
    public IActionResult Ingresar(string? returnUrl = null) => View(new InicioSesionViewModel { ReturnUrl = returnUrl });

    /// <summary>Valida credenciales y emite una cookie con identidad y rol.</summary>
    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Ingresar(InicioSesionViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        try
        {
            var session = await authService.IniciarSesionAsync(
                new InicioSesionDto { CorreoElectronico = modelo.CorreoElectronico, Contrasena = modelo.Contrasena },
                cancellationToken);
            await EmitirCookieAsync(session.Id, session.NombreCompleto, session.CorreoElectronico, session.Rol.ToString());
            return LocalRedirect(Url.IsLocalUrl(modelo.ReturnUrl) ? modelo.ReturnUrl! : Url.Action("Index", "Home")!);
        }
        catch (BusinessRuleException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(modelo);
        }
        catch (ValidationException exception)
        {
            foreach (var error in exception.Errors)
            {
                ModelState.AddModelError(string.Empty, error.ErrorMessage);
            }

            return View(modelo);
        }
    }

    /// <summary>Muestra el formulario de registro de adoptante.</summary>
    [AllowAnonymous, HttpGet]
    public IActionResult Registrar() => View(new RegistroViewModel());

    /// <summary>Crea un adoptante y lo autentica mediante cookie.</summary>
    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(RegistroViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        try
        {
            var session = await authService.RegistrarAsync(new RegistroUsuarioDto
            {
                NombreCompleto = modelo.NombreCompleto,
                CorreoElectronico = modelo.CorreoElectronico,
                Contrasena = modelo.Contrasena
            }, cancellationToken);
            await EmitirCookieAsync(session.Id, session.NombreCompleto, session.CorreoElectronico, session.Rol.ToString());
            TempData["Mensaje"] = "Tu cuenta está lista. ¡Bienvenido a PetFinder!";
            return RedirectToAction("Index", "Home");
        }
        catch (BusinessRuleException exception)
        {
            ModelState.AddModelError(nameof(modelo.CorreoElectronico), exception.Message);
            return View(modelo);
        }
        catch (ValidationException exception)
        {
            foreach (var error in exception.Errors)
            {
                ModelState.AddModelError(string.Empty, error.ErrorMessage);
            }

            return View(modelo);
        }
    }

    /// <summary>Muestra el selector de roles de prueba sin credenciales, solo en Development.</summary>
    [AllowAnonymous, HttpGet]
    public IActionResult Demo()
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        return View(new DemoLoginViewModel());
    }

    /// <summary>Emite una cookie local con una identidad seed; este acceso no se publica fuera de Development.</summary>
    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Demo(Rol rol)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        var demo = rol switch
        {
            Rol.Adoptante => (Guid.Parse("33333333-3333-3333-3333-333333333333"), "Adoptante de ejemplo", "adoptante@petfinder.local"),
            Rol.Refugio => (Guid.Parse("22222222-2222-2222-2222-222222222222"), "Equipo Huellitas", "refugio@petfinder.local"),
            Rol.Administrador => (Guid.Parse("11111111-1111-1111-1111-111111111111"), "Administración PetFinder", "admin@petfinder.local"),
            _ => (Guid.Empty, string.Empty, string.Empty)
        };
        if (demo.Item1 == Guid.Empty)
        {
            ModelState.AddModelError(string.Empty, "Selecciona uno de los roles de demostración.");
            return View(new DemoLoginViewModel { Rol = rol });
        }

        await EmitirCookieAsync(demo.Item1, demo.Item2, demo.Item3, rol.ToString());
        logger.LogWarning("Se usó la autenticación simulada de desarrollo para el rol {Rol}.", rol);
        return RedirectToAction("Index", "Home");
    }

    /// <summary>Finaliza la sesión y elimina la cookie de autenticación.</summary>
    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Salir()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    /// <summary>Presenta el destino cuando el rol de la cookie no tiene acceso a una ruta.</summary>
    [AllowAnonymous, HttpGet]
    public IActionResult AccesoDenegado() => View();

    private async Task EmitirCookieAsync(Guid id, string nombre, string correo, string rol)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, id.ToString()),
            new Claim(ClaimTypes.Name, nombre),
            new Claim(ClaimTypes.Email, correo),
            new Claim(ClaimTypes.Role, rol)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true, AllowRefresh = true });
    }
}
