using MediatR;
using Pagos.Application.DTOs;

namespace Pagos.Application.Commands
{
    public class AgregarMPagoCommand : IRequest<String>
    {
        public AgregarMPagoStripeDTO MPagoStripe { get; set; }
        public AgregarMPagoCommand(AgregarMPagoStripeDTO mPagoStripe)
        {
            MPagoStripe = mPagoStripe;
        }
    }
}
