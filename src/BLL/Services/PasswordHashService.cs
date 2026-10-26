using System.Globalization;
using System.Security.Cryptography;

namespace PetFinder.BLL.Services;

/// <summary>Protege contraseñas con PBKDF2-SHA256 y una sal aleatoria por usuario.</summary>
internal static class PasswordHashService
{
    private const int Iteraciones = 210_000;
    private const int TamanoSal = 16;
    private const int TamanoHash = 32;

    public static string CrearHash(string contrasena)
    {
        var sal = RandomNumberGenerator.GetBytes(TamanoSal);
        var hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, Iteraciones, HashAlgorithmName.SHA256, TamanoHash);
        return $"PBKDF2-SHA256${Iteraciones.ToString(CultureInfo.InvariantCulture)}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verificar(string contrasena, string hashAlmacenado)
    {
        var segmentos = hashAlmacenado.Split('$');
        if (segmentos.Length != 4
            || segmentos[0] != "PBKDF2-SHA256"
            || !int.TryParse(segmentos[1], NumberStyles.None, CultureInfo.InvariantCulture, out var iteraciones)
            || iteraciones is < 100_000 or > 1_000_000)
        {
            return false;
        }

        try
        {
            var sal = Convert.FromBase64String(segmentos[2]);
            var hashEsperado = Convert.FromBase64String(segmentos[3]);
            var hashActual = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, iteraciones, HashAlgorithmName.SHA256, hashEsperado.Length);
            return CryptographicOperations.FixedTimeEquals(hashActual, hashEsperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
