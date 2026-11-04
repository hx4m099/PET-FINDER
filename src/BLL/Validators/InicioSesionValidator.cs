using FluentValidation;
using PetFinder.BLL.Dtos;
using PetFinder.Entity.Constants;

namespace PetFinder.BLL.Validators;

/// <summary>Valida las credenciales recibidas en el inicio de sesión.</summary>
public sealed class InicioSesionValidator : AbstractValidator<InicioSesionDto>
{
    public InicioSesionValidator()
    {
        RuleFor(dto => dto.CorreoElectronico).NotEmpty().EmailAddress().MaximumLength(LimitesEntidad.CorreoElectronico);
        RuleFor(dto => dto.Contrasena).NotEmpty().MaximumLength(128);
    }
}
