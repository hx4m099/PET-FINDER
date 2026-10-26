namespace PetFinder.BLL.Exceptions;

/// <summary>Indica que una operación infringe una regla de negocio.</summary>
public sealed class BusinessRuleException(string message) : Exception(message);
