namespace PetFinder.Entity.Constants;

/// <summary>
/// Límites de longitud de los campos de texto. Deben coincidir con los VARCHAR2 de
/// sql/01_crear_tablas.sql.
/// Fuente única: ViewModels, DTOs, validadores y configuraciones de EF Core las usan.
/// </summary>
public static class LimitesEntidad
{
    public const int NombreUsuario = 30;
    public const int NombreRefugio = 30;
    public const int NombreMascota = 20;
    public const int CorreoElectronico = 40;
    public const int PasswordHash = 100;
    public const int Telefono = 15;
    public const int Horario = 40;
    public const int Ciudad = 25;
    public const int Direccion = 50;
    public const int Raza = 25;
    public const int Url = 80;
    public const int Descripcion = 150;
    public const int Notas = 100;
}
