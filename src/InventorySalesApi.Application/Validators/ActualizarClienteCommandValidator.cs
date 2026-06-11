using FluentValidation;
using InventorySalesApi.Application.Features.Clientes.Commands;

namespace InventorySalesApi.Application.Validators;

public class ActualizarClienteCommandValidator : AbstractValidator<ActualizarClienteCommand>
{
    public ActualizarClienteCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID del cliente es requerido.");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del cliente no puede estar vacío.")
            .MaximumLength(100).WithMessage("El nombre del cliente no puede superar los 100 caracteres.");

        RuleFor(x => x.Apellido)
            .NotEmpty().WithMessage("El apellido del cliente no puede estar vacío.")
            .MaximumLength(100).WithMessage("El apellido del cliente no puede superar los 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es requerido.")
            .EmailAddress().WithMessage("El correo electrónico no tiene un formato válido.");

        RuleFor(x => x.Telefono)
            .MaximumLength(30).WithMessage("El teléfono no puede superar los 30 caracteres.");

        RuleFor(x => x.DocumentoIdentidad)
            .NotEmpty().WithMessage("El documento de identidad del cliente es requerido.")
            .MaximumLength(30).WithMessage("El documento de identidad no puede superar los 30 caracteres.");
    }
}
