using FluentValidation;
using Pagos.Application.DTOs;

namespace Pagos.Application.Validations
{
    public class AgregarPagoDTOValidator : AbstractValidator<AgregarPagoDTO>
    {
        public AgregarPagoDTOValidator()
        {
            RuleFor(x => x.IdMPago)
                .NotNull().WithMessage("El Id de MPago es obligatorio.")
                .NotEmpty().WithMessage("El Id de MPago es obligatorio.")
                .Length(1, 50).WithMessage("El Id de MPago debe tener entre 1 y 50 caracteres.");

            RuleFor(x => x.IdUsuario)
                .NotNull().WithMessage("El Id del Usuario es obligatorio.")
                .NotEmpty().WithMessage("El Id del Usuario es obligatorio.")
                .Length(1, 50).WithMessage("El Id del Usuario debe tener entre 1 y 50 caracteres.");

            RuleFor(x => x.IdReserva)
                .NotNull().WithMessage("El Id de la Reserva es obligatorio.")
                .NotEmpty().WithMessage("El Id de la Reserva es obligatorio.")
                .Length(1, 50).WithMessage("El Id de la Reserva debe tener entre 1 y 50 caracteres.");

            RuleFor(x => x.IdEvento)
                .NotNull().WithMessage("El Id del Evento es obligatorio.")
                .NotEmpty().WithMessage("El Id del Evento es obligatorio.")
                .Length(1, 50).WithMessage("El Id del Evento debe tener entre 1 y 50 caracteres.");

            RuleFor(x => x.FechaPago)
                .NotNull().WithMessage("La Fecha de registro es obligatoria.")
                .NotEmpty().WithMessage("La Fecha de registro es obligatoria.");

            RuleFor(x => x.Monto)
                .NotNull().WithMessage("El campo Monto es obligatorio.")
                .NotEmpty().WithMessage("El campo Monto es obligatorio.")
                .GreaterThan(0).WithMessage("El campo Monto debe ser mayor que cero.");

        }
    }
}
