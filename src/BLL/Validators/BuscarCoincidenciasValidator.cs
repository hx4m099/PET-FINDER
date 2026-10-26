using FluentValidation;
using PetFinder.BLL.Dtos;

namespace PetFinder.BLL.Validators;

/// <summary>Valida parámetros de búsqueda de reportes compatibles.</summary>
public sealed class BuscarCoincidenciasValidator : AbstractValidator<BuscarCoincidenciasDto>
{
    public BuscarCoincidenciasValidator()
    {
        RuleFor(dto => dto.Especie).IsInEnum();
        RuleFor(dto => dto.Ciudad).NotEmpty().MinimumLength(2).MaximumLength(100);
        RuleFor(dto => dto.DiasRecientes).InclusiveBetween(1, 365);
        RuleFor(dto => dto.TipoReporteOrigen).IsInEnum().When(dto => dto.TipoReporteOrigen.HasValue);
    }
}
