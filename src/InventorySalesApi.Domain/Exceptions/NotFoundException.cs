using System;

namespace InventorySalesApi.Domain.Exceptions;

/// <summary>
/// Excepción personalizada para representar errores cuando un recurso no es encontrado en el sistema.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
