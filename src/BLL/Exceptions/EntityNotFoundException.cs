namespace PetFinder.BLL.Exceptions;

/// <summary>Indica que no se encontró la entidad solicitada.</summary>
public sealed class EntityNotFoundException(string entityName, Guid id)
    : Exception($"No se encontró {entityName} con identificador {id}.");
