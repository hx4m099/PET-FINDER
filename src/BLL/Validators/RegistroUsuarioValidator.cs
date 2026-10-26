using FluentValidation;
using PetFinder.BLL.Dtos;

namespace PetFinder.BLL.Validators;

/// <summary>Valida los datos de registro antes de crear una cuenta.</summary>
public sealed class RegistroUsuarioValidator : AbstractValidator<RegistroUsuarioDto>
{
    public RegistroUsuarioValidator()
    {
        RuleFor(dto => dto.NombreCompleto).NotEmpty().MinimumLength(2).MaximumLength(120);
        RuleFor(dto => dto.CorreoElectronico).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(dto => dto.Contrasena)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128)
            .Matches("[A-Z]").WithMessage("La contraseña debe contener al menos una mayúscula.")
            .Matches("[a-z]").WithMessage("La contraseña debe contener al menos una minúscula.")
            .Matches("[0-9]").WithMessage("La contraseña debe contener al menos un número.");
    }
}
