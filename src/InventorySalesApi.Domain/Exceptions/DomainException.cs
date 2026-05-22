using System;

namespace InventorySalesApi.Domain.Exceptions;

/// <summary>
/// Excepción personalizada para representar errores específicos del dominio.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
