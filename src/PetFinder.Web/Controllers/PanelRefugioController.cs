using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetFinder.BLL.Dtos;
using PetFinder.BLL.Exceptions;
using PetFinder.BLL.Services;
using PetFinder.Entity.Enums;
using PetFinder.Web.ViewModels;

namespace PetFinder.Web.Controllers;

/// <summary>Herramientas de gestión de un refugio aliado.</summary>
[Authorize(Roles = "Refugio")]
public sealed class PanelRefugioController(GestionRefugioService gestionRefugioService) : Controller
{
    /// <summary>Muestra el refugio y sus mascotas publicadas.</summary>
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var panel = await gestionRefugioService.ObtenerPanelAsync(ObtenerUsuarioId(), cancellationToken);
        return View(new PanelRefugioViewModel(panel.Refugio, panel.Mascotas));
    }

    /// <summary>Muestra el formulario para crear una publicación de adopción.</summary>
    [HttpGet]
    public IActionResult CrearMascota() => View("EditarMascota", new MascotaRefugioViewModel());

    /// <summary>Muestra el formulario de edición de una mascota del refugio actual.</summary>
    [HttpGet]
    public async Task<IActionResult> EditarMascota(Guid id, CancellationToken cancellationToken)
    {
        var panel = await gestionRefugioService.ObtenerPanelAsync(ObtenerUsuarioId(), cancellationToken);
        var mascota = panel.Mascotas.FirstOrDefault(item => item.Id == id)
            ?? throw new EntityNotFoundException("mascota del refugio", id);
        return View(new MascotaRefugioViewModel
        {
            Id = mascota.Id,
            Nombre = mascota.Nombre,
            Especie = mascota.Especie,
            Raza = mascota.Raza,
            EdadMeses = mascota.EdadMeses,
            Descripcion = mascota.Descripcion,
            FotoUrl = mascota.FotoUrl,
            Ciudad = mascota.Ciudad,
            DisponibleAdopcion = mascota.DisponibleAdopcion
        });
    }

    /// <summary>Guarda una mascota nueva o actualiza una publicación propia.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GuardarMascota(MascotaRefugioViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("EditarMascota", modelo);
        }

        try
        {
            await gestionRefugioService.GuardarMascotaAsync(ObtenerUsuarioId(), new GuardarMascotaRefugioDto(
                modelo.Id,
                modelo.Nombre,
                modelo.Especie,
                modelo.Raza,
                modelo.EdadMeses,
                modelo.Descripcion,
                modelo.FotoUrl,
                modelo.Ciudad,
                modelo.DisponibleAdopcion), cancellationToken);
            TempData["Mensaje"] = "La publicación se guardó correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View("EditarMascota", modelo);
        }
        catch (ValidationException exception)
        {
            foreach (var error in exception.Errors)
            {
                ModelState.AddModelError(string.Empty, error.ErrorMessage);
            }

            return View("EditarMascota", modelo);
        }
    }

    private Guid ObtenerUsuarioId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new BusinessRuleException("La sesión no contiene un identificador válido.");
}
