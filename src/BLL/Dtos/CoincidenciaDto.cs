using PetFinder.Entity.Entities;

namespace PetFinder.BLL.Dtos;

/// <summary>Reporte candidato acompañado de un porcentaje de similitud.</summary>
public sealed record CoincidenciaDto(ReporteMascota Reporte, decimal PorcentajeSimilitud);
