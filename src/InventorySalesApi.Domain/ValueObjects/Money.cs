using System;
using InventorySalesApi.Domain.Exceptions;

namespace InventorySalesApi.Domain.ValueObjects;

/// <summary>
/// Representa un valor monetario inmutable con validación y operadores.
/// </summary>
public readonly record struct Money
{
    public decimal Amount { get; }

    public Money(decimal amount)
    {
        if (amount < 0)
        {
            throw new DomainException("El monto no puede ser negativo.");
        }
        Amount = amount;
    }

    public static Money Zero => new(0);

    // Sobrecarga de operadores aritméticos
    public static Money operator +(Money a, Money b) => new(a.Amount + b.Amount);
    
    public static Money operator -(Money a, Money b)
    {
        if (a.Amount < b.Amount)
        {
            throw new DomainException("La resta no puede dar un resultado negativo en este contexto de negocio.");
        }
        return new(a.Amount - b.Amount);
    }
    
    public static Money operator *(Money a, decimal factor) => new(a.Amount * factor);
    public static Money operator *(decimal factor, Money a) => new(a.Amount * factor);

    // Sobrecarga de operadores de comparación
    public static bool operator <(Money a, Money b) => a.Amount < b.Amount;
    public static bool operator >(Money a, Money b) => a.Amount > b.Amount;
    public static bool operator <=(Money a, Money b) => a.Amount <= b.Amount;
    public static bool operator >=(Money a, Money b) => a.Amount >= b.Amount;

    // Conversiones implícitas/explícitas para facilitar la interoperabilidad
    public static implicit operator decimal(Money money) => money.Amount;
    public static explicit operator Money(decimal amount) => new(amount);

    public override string ToString() => $"${Amount:N2}";
}
