using PetFinder.Entity.Entities;

namespace PetFinder.Web.ViewModels;

/// <summary>Resumen administrativo de cuentas y refugios de PetFinder.</summary>
public sealed record PanelAdministradorViewModel(
    int TotalUsuarios,
    int UsuariosActivos,
    int TotalRefugios,
    int RefugiosActivos,
    IReadOnlyCollection<Usuario> Usuarios,
    IReadOnlyCollection<Refugio> Refugios);
