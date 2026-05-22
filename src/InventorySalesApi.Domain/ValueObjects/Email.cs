using System.Text.RegularExpressions;
using InventorySalesApi.Domain.Exceptions;

namespace InventorySalesApi.Domain.ValueObjects;

/// <summary>
/// Representa una dirección de correo electrónico válida e inmutable.
/// </summary>
public readonly record struct Email
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("El correo electrónico no puede estar vacío o contener solo espacios en blanco.");
        }

        string trimmedValue = value.Trim();

        if (!EmailRegex.IsMatch(trimmedValue))
        {
            throw new DomainException($"El formato del correo electrónico '{value}' es inválido.");
        }

        Value = trimmedValue;
    }

    public static implicit operator string(Email email) => email.Value;
    public static explicit operator Email(string value) => new(value);

    public override string ToString() => Value;
}
