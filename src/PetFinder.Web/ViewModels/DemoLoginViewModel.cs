using PetFinder.Entity.Enums;

namespace PetFinder.Web.ViewModels;

/// <summary>Rol seed que se usará para una cookie de demostración solo local.</summary>
public sealed class DemoLoginViewModel
{
    public Rol Rol { get; set; } = Rol.Adoptante;
}
