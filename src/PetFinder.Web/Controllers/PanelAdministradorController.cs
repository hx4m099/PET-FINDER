using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetFinder.BLL.Exceptions;
using PetFinder.BLL.Services;
using PetFinder.Web.ViewModels;

namespace PetFinder.Web.Controllers;

/// <summary>Herramientas básicas de moderación disponibles para administradores.</summary>
[Authorize(Roles = "Administrador")]
public sealed class PanelAdministradorController(AdministracionService administracionService) : Controller
{
    /// <summary>Presenta conteos y listados de cuentas y refugios.</summary>
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var resumen = await administracionService.ObtenerResumenAsync(ObtenerUsuarioId(), cancellationToken);
        return View(new PanelAdministradorViewModel(
            resumen.Usuarios.Count,
            resumen.Usuarios.Count(usuario => usuario.Activo),
            resumen.Refugios.Count,
            resumen.Refugios.Count(refugio => refugio.Activo),
            resumen.Usuarios,
            resumen.Refugios));
    }

    /// <summary>Cambia el estado activo de una cuenta de usuario.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstadoUsuario(Guid id, bool activo, CancellationToken cancellationToken)
    {
        await administracionService.CambiarEstadoUsuarioAsync(ObtenerUsuarioId(), id, activo, cancellationToken);
        TempData["Mensaje"] = activo ? "Cuenta reactivada." : "Cuenta suspendida.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Cambia el estado activo de un refugio aliado.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstadoRefugio(Guid id, bool activo, CancellationToken cancellationToken)
    {
        await administracionService.CambiarEstadoRefugioAsync(ObtenerUsuarioId(), id, activo, cancellationToken);
        TempData["Mensaje"] = activo ? "Refugio reactivado." : "Refugio suspendido.";
        return RedirectToAction(nameof(Index));
    }

    private Guid ObtenerUsuarioId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new BusinessRuleException("La sesión no contiene un identificador válido.");
}
