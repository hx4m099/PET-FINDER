using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetFinder.BLL.Exceptions;
using PetFinder.BLL.Services;
using PetFinder.Entity.Enums;

namespace PetFinder.Web.Controllers;

/// <summary>Controla el listado personal y las acciones autorizadas sobre citas.</summary>
[Authorize]
public sealed class CitasController(CitaService citaService) : Controller
{
    /// <summary>Muestra las citas del usuario actual.</summary>
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var usuarioId = ObtenerUsuarioId();
        var citas = User.IsInRole("Refugio")
            ? await citaService.ObtenerPorRefugioAsync(usuarioId, cancellationToken)
            : await citaService.ObtenerPorUsuarioAsync(usuarioId, cancellationToken);
        return View(citas);
    }

    /// <summary>Permite que el refugio o el administrador confirme una cita pendiente.</summary>
    [Authorize(Roles = "Refugio,Administrador"), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirmar(Guid id, CancellationToken cancellationToken)
    {
        await citaService.ConfirmarAsync(id, ObtenerUsuarioId(), cancellationToken);
        TempData["Mensaje"] = "La cita quedó confirmada.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Permite cancelar una cita pendiente o confirmada conforme al rol.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(Guid id, CancellationToken cancellationToken)
    {
        await citaService.CancelarAsync(id, ObtenerUsuarioId(), cancellationToken);
        TempData["Mensaje"] = "La cita fue cancelada.";
        return RedirectToAction(nameof(Index));
    }

    private Guid ObtenerUsuarioId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new BusinessRuleException("La sesión no contiene un identificador válido.");
}
