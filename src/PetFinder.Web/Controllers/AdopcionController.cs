using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetFinder.BLL.Dtos;
using PetFinder.BLL.Exceptions;
using PetFinder.BLL.Services;
using PetFinder.Web.ViewModels;

namespace PetFinder.Web.Controllers;

/// <summary>Controla el catálogo público de adopción y las solicitudes de visita.</summary>
public sealed class AdopcionController(AdopcionService adopcionService, CitaService citaService) : Controller
{
    /// <summary>Lista mascotas disponibles y filtra por refugio, especie y edad.</summary>
    [AllowAnonymous, HttpGet]
    public async Task<IActionResult> Index(CatalogoViewModel filtros, CancellationToken cancellationToken)
    {
        filtros.Refugios = await adopcionService.ObtenerRefugiosActivosAsync(cancellationToken);
        filtros.Mascotas = await adopcionService.ObtenerCatalogoAsync(
            filtros.RefugioId,
            filtros.Especie,
            filtros.EdadMinimaMeses,
            filtros.EdadMaximaMeses,
            cancellationToken);
        return View(filtros);
    }

    /// <summary>Muestra el detalle de una publicación activa.</summary>
    [AllowAnonymous, HttpGet]
    public async Task<IActionResult> Detalle(Guid id, CancellationToken cancellationToken)
    {
        var (mascota, refugio) = await adopcionService.ObtenerDetalleAsync(id, cancellationToken);
        return View(new DetalleMascotaViewModel { Mascota = mascota, Refugio = refugio });
    }

    /// <summary>Reserva la cita solicitada por el usuario autenticado.</summary>
    [Authorize(Roles = "Adoptante"), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Agendar(Guid id, DetalleMascotaViewModel modelo, CancellationToken cancellationToken)
    {
        var (mascota, refugio) = await adopcionService.ObtenerDetalleAsync(id, cancellationToken);
        if (!ModelState.IsValid)
        {
            modelo.Mascota = mascota;
            modelo.Refugio = refugio;
            return View("Detalle", modelo);
        }

        try
        {
            var usuarioId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var fechaUtc = modelo.FechaInicioLocal.Kind == DateTimeKind.Utc
                ? modelo.FechaInicioLocal
                : modelo.FechaInicioLocal.ToUniversalTime();
            await citaService.SolicitarAsync(new SolicitarCitaDto
            {
                UsuarioId = usuarioId,
                RefugioId = refugio.Id,
                MascotaId = mascota.Id,
                FechaInicioUtc = fechaUtc,
                DuracionMinutos = modelo.DuracionMinutos,
                Notas = modelo.Notas
            }, cancellationToken);
            TempData["Mensaje"] = "Tu solicitud de cita quedó pendiente de confirmación.";
            return RedirectToAction("Index", "Citas");
        }
        catch (BusinessRuleException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            modelo.Mascota = mascota;
            modelo.Refugio = refugio;
            return View("Detalle", modelo);
        }
        catch (ValidationException exception)
        {
            foreach (var error in exception.Errors)
            {
                ModelState.AddModelError(string.Empty, error.ErrorMessage);
            }

            modelo.Mascota = mascota;
            modelo.Refugio = refugio;
            return View("Detalle", modelo);
        }
    }
}
