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

/// <summary>Controla la publicación de reportes y la presentación de coincidencias.</summary>
[Authorize]
public sealed class ReportesController(
    ReporteService reporteService,
    CoincidenciaService coincidenciaService,
    IWebHostEnvironment environment) : Controller
{
    private const long TamanoMaximoFoto = 5 * 1024 * 1024;

    /// <summary>Muestra el formulario de reporte.</summary>
    [HttpGet]
    public IActionResult Crear() => View(new ReporteViewModel());

    /// <summary>Valida y publica el reporte asociado al adoptante actual.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ReporteViewModel modelo, CancellationToken cancellationToken)
    {
        if (modelo.Foto is null && string.IsNullOrWhiteSpace(modelo.FotoUrl))
        {
            ModelState.AddModelError(nameof(modelo.Foto), "Selecciona una foto o proporciona una URL de imagen.");
        }

        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        string? rutaGuardada = null;
        try
        {
            var userId = ObtenerUsuarioId();
            if (modelo.Foto is not null)
            {
                if (modelo.Foto.Length is <= 0 or > TamanoMaximoFoto)
                {
                    ModelState.AddModelError(nameof(modelo.Foto), "La foto debe pesar como máximo 5 MB.");
                    return View(modelo);
                }

                var extension = modelo.Foto.ContentType.ToLowerInvariant() switch
                {
                    "image/jpeg" => ".jpg",
                    "image/png" => ".png",
                    "image/webp" => ".webp",
                    _ => string.Empty
                };
                if (extension.Length == 0)
                {
                    ModelState.AddModelError(nameof(modelo.Foto), "Solo se admiten imágenes JPG, PNG o WebP.");
                    return View(modelo);
                }

                var nombreArchivo = $"{Guid.NewGuid():N}{extension}";
                var directorio = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "uploads", "reports");
                Directory.CreateDirectory(directorio);
                var destino = Path.Combine(directorio, nombreArchivo);
                await using (var stream = System.IO.File.Create(destino))
                {
                    await modelo.Foto.CopyToAsync(stream, cancellationToken);
                }

                rutaGuardada = destino;
                modelo.FotoUrl = $"/uploads/reports/{nombreArchivo}";
            }

            var fechaUtc = modelo.FechaEvento.Kind == DateTimeKind.Utc
                ? modelo.FechaEvento
                : modelo.FechaEvento.ToUniversalTime();
            var reporte = await reporteService.CrearAsync(userId, new CrearReporteMascotaDto
            {
                Tipo = modelo.Tipo,
                Especie = modelo.Especie,
                NombreMascota = modelo.NombreMascota,
                Raza = modelo.Raza,
                Ciudad = modelo.Ciudad,
                Descripcion = modelo.Descripcion,
                FotoUrl = modelo.FotoUrl,
                FechaEventoUtc = fechaUtc
            }, cancellationToken);
            return RedirectToAction(nameof(Resultados), new { id = reporte.Id });
        }
        catch (BusinessRuleException exception)
        {
            EliminarFoto(rutaGuardada);
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(modelo);
        }
        catch (ValidationException exception)
        {
            EliminarFoto(rutaGuardada);
            foreach (var error in exception.Errors)
            {
                ModelState.AddModelError(string.Empty, error.ErrorMessage);
            }

            return View(modelo);
        }
    }

    /// <summary>Busca y ordena los reportes opuestos más parecidos al reporte indicado.</summary>
    [HttpGet]
    public async Task<IActionResult> Resultados(Guid id, CancellationToken cancellationToken)
    {
        var coincidencias = await coincidenciaService.BuscarParaReporteAsync(id, cancellationToken: cancellationToken);
        ViewData["ReporteId"] = id;
        ViewData["EsSimulado"] = true;
        return View(coincidencias);
    }

    private Guid ObtenerUsuarioId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new BusinessRuleException("La sesión no contiene un identificador válido.");

    private static void EliminarFoto(string? ruta)
    {
        if (ruta is not null && System.IO.File.Exists(ruta))
        {
            System.IO.File.Delete(ruta);
        }
    }
}
