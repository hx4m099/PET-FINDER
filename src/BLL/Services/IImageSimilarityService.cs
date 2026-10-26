namespace PetFinder.BLL.Services;

/// <summary>Calcula una puntuación de similitud entre dos imágenes.</summary>
public interface IImageSimilarityService
{
    /// <summary>Devuelve un porcentaje entre 0 y 100 para dos referencias de imagen.</summary>
    Task<decimal> CalcularPorcentajeAsync(
        string imagenReferencia,
        string imagenCandidata,
        CancellationToken cancellationToken = default);
}
