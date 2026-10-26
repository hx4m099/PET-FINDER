using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PetFinder.BLL.Exceptions;

namespace PetFinder.Web.Middleware;

/// <summary>Traduce excepciones de aplicación a respuestas HTTP consistentes sin exponer detalles internos.</summary>
public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            var (statusCode, title, detail) = exception switch
            {
                ValidationException validation => (StatusCodes.Status400BadRequest, "Datos no válidos", string.Join(" ", validation.Errors.Select(error => error.ErrorMessage).Distinct())),
                BusinessRuleException business => (StatusCodes.Status400BadRequest, "No se pudo completar la operación", business.Message),
                EntityNotFoundException missing => (StatusCodes.Status404NotFound, "No encontrado", missing.Message),
                OperationCanceledException when context.RequestAborted.IsCancellationRequested => (499, "Solicitud cancelada", "La operación fue cancelada."),
                _ => (StatusCodes.Status500InternalServerError, "Error interno", "Ocurrió un error inesperado. Inténtalo de nuevo más tarde.")
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "Error no controlado al procesar {Method} {Path}.", context.Request.Method, context.Request.Path);
            }
            else
            {
                logger.LogInformation("La solicitud {Path} terminó con HTTP {StatusCode}: {Title}.", context.Request.Path, statusCode, title);
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            if (context.Request.Headers.Accept.Any(value => value?.Contains("text/html", StringComparison.OrdinalIgnoreCase) == true))
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync($"<!doctype html><html lang=\"es\"><meta charset=\"utf-8\"><title>{System.Net.WebUtility.HtmlEncode(title)}</title><body><h1>{System.Net.WebUtility.HtmlEncode(title)}</h1><p>{System.Net.WebUtility.HtmlEncode(detail)}</p><a href=\"/\">Volver al inicio</a></body></html>");
                return;
            }

            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            });
        }
    }
}
