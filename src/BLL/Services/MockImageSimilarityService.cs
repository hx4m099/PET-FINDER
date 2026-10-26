using System.Security.Cryptography;
using System.Text;

namespace PetFinder.BLL.Services;

/// <summary>Genera puntuaciones deterministas de demostración; no analiza píxeles ni contenido visual.</summary>
public sealed class MockImageSimilarityService : IImageSimilarityService
{
    /// <inheritdoc />
    public Task<decimal> CalcularPorcentajeAsync(
        string imagenReferencia,
        string imagenCandidata,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(imagenReferencia);
        ArgumentException.ThrowIfNullOrWhiteSpace(imagenCandidata);
        cancellationToken.ThrowIfCancellationRequested();

        var par = string.CompareOrdinal(imagenReferencia, imagenCandidata) <= 0
            ? $"{imagenReferencia}\n{imagenCandidata}"
            : $"{imagenCandidata}\n{imagenReferencia}";
        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(par));
        var valor = BitConverter.ToUInt16(digest, 0) % 41;
        return Task.FromResult(55m + valor);
    }
}
