using PetFinder.Entity.Entities;

namespace PetFinder.Web.ViewModels;

/// <summary>Resumen del refugio y mascotas administradas por su cuenta.</summary>
public sealed record PanelRefugioViewModel(Refugio Refugio, IReadOnlyCollection<Mascota> Mascotas);
