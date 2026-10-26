namespace PetFinder.Entity.Constants;

/// <summary>Límites compartidos para campos de las entidades del dominio.</summary>
public static class LimitesEntidad
{
    public const int Nombre = 120;
    public const int CorreoElectronico = 254;
    public const int PasswordHash = 512;
    public const int Telefono = 32;
    public const int Ciudad = 100;
    public const int Direccion = 250;
    public const int Raza = 100;
    public const int Url = 2048;
    public const int Descripcion = 2000;
    public const int Notas = 1000;
}
