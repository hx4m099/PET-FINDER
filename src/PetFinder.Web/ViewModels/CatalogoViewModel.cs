using System.ComponentModel.DataAnnotations;
using PetFinder.Entity.Enums;

namespace PetFinder.Web.ViewModels;

/// <summary>Filtros y resultados del catálogo de adopción.</summary>
public sealed class CatalogoViewModel
{
    public Especie? Especie { get; set; }
    public Guid? RefugioId { get; set; }

    [Range(0, 1000)]
    public int? EdadMinimaMeses { get; set; }

    [Range(0, 1000)]
    public int? EdadMaximaMeses { get; set; }

    public IReadOnlyCollection<PetFinder.Entity.Entities.Refugio> Refugios { get; set; } = [];
    public IReadOnlyCollection<PetFinder.Entity.Entities.Mascota> Mascotas { get; set; } = [];
}
