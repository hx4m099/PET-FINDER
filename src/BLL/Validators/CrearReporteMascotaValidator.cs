using FluentValidation;
using PetFinder.BLL.Dtos;
using PetFinder.Entity.Enums;
using PetFinder.Entity.Constants;

namespace PetFinder.BLL.Validators;

/// <summary>Valida el contenido de reportes de mascotas.</summary>
public sealed class CrearReporteMascotaValidator : AbstractValidator<CrearReporteMascotaDto>
{
    public CrearReporteMascotaValidator()
    {
        RuleFor(dto => dto.Tipo).IsInEnum();
        RuleFor(dto => dto.Especie).IsInEnum();
        RuleFor(dto => dto.NombreMascota).MaximumLength(LimitesEntidad.NombreMascota);
        RuleFor(dto => dto.Raza).MaximumLength(LimitesEntidad.Raza);
        RuleFor(dto => dto.Ciudad).NotEmpty().MinimumLength(2).MaximumLength(LimitesEntidad.Ciudad);
        RuleFor(dto => dto.Descripcion).NotEmpty().MinimumLength(5).MaximumLength(LimitesEntidad.Descripcion);
        RuleFor(dto => dto.FotoUrl)
            .NotEmpty()
            .MaximumLength(LimitesEntidad.Url)
            .Must(EsReferenciaDeImagenValida)
            .WithMessage("La foto debe ser una URL HTTP(S) o una ruta local válida.");
        RuleFor(dto => dto.FechaEventoUtc).Must(fecha => fecha != default)
            .WithMessage("La fecha del evento es obligatoria.");
    }

    private static bool EsReferenciaDeImagenValida(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return false;
        }

        if (Uri.TryCreate(valor, UriKind.Absolute, out var uri))
        {
            return uri.Scheme is "http" or "https";
        }

        return valor.StartsWith('/') && !valor.Contains("..", StringComparison.Ordinal);
    }
}
