using FluentValidation;
using PetFinder.BLL.Dtos;

namespace PetFinder.BLL.Validators;

/// <summary>Valida los campos de una solicitud de cita.</summary>
public sealed class SolicitarCitaValidator : AbstractValidator<SolicitarCitaDto>
{
    public SolicitarCitaValidator()
    {
        RuleFor(dto => dto.UsuarioId).NotEmpty();
        RuleFor(dto => dto.RefugioId).NotEmpty();
        RuleFor(dto => dto.MascotaId).NotEmpty();
        RuleFor(dto => dto.FechaInicioUtc).Must(fecha => fecha != default)
            .WithMessage("La fecha y hora de inicio son obligatorias.");
        RuleFor(dto => dto.DuracionMinutos).InclusiveBetween(15, 120);
        RuleFor(dto => dto.Notas).MaximumLength(1000);
    }
}
