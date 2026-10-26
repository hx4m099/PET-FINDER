using FluentValidation;
using PetFinder.BLL.Dtos;

namespace PetFinder.BLL.Validators;

/// <summary>Valida las credenciales recibidas en el inicio de sesión.</summary>
public sealed class InicioSesionValidator : AbstractValidator<InicioSesionDto>
{
    public InicioSesionValidator()
    {
        RuleFor(dto => dto.CorreoElectronico).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(dto => dto.Contrasena).NotEmpty().MaximumLength(128);
    }
}
