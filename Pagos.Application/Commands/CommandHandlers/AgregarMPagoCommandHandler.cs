using MediatR;
using Pagos.Application.Interfaces;
using Pagos.Domain.Entities;
using Pagos.Domain.Exceptions;
using Pagos.Domain.Interfaces;
using Pagos.Domain.ValueObjects;
using Stripe;

namespace Pagos.Application.Commands.CommandHandlers
{
    public class AgregarMPagoCommandHandler : IRequestHandler<AgregarMPagoCommand, string>
    {
        private readonly IMPagoRepository MPagoWriteRepository;
        private readonly IStripeService StripeService;
        private readonly ITarjetaCreditoFactory TarjetaCreditoFactory;

        public AgregarMPagoCommandHandler(IMPagoRepository mPagoWriteRepository, IStripeService stripeService, ITarjetaCreditoFactory tarjetaCreditoFactory)
        {
            MPagoWriteRepository = mPagoWriteRepository ?? throw new MPagoWriteRepositoryNullException();
            StripeService = stripeService ?? throw new StripeServiceNullException();
            TarjetaCreditoFactory = tarjetaCreditoFactory;//?? throw new TarjetaCreditoFactoryNullException();
        }

        public async Task<string> Handle(AgregarMPagoCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var mPagoStripeDto = request.MPagoStripe;
                if (string.IsNullOrWhiteSpace(mPagoStripeDto.IdMPagoStripe))
                {
                    throw new IdMPagoStripeNullCommandHandlerException();
                }

                // Crear cliente en Stripe y asociar el método de pago
                var customer = await StripeService.CrearTokenCUS(mPagoStripeDto.CorreoUsuario, mPagoStripeDto.IdMPagoStripe);
                // Obtener detalles del método de pago desde Stripe
                var paymentMethodService = new PaymentMethodService();
                var paymentMethod = await paymentMethodService.GetAsync(mPagoStripeDto.IdMPagoStripe);

                var mPago = TarjetaCreditoFactory.Crear(
                    mPagoStripeDto.IdUsuario,
                    mPagoStripeDto.IdMPagoStripe,
                    customer.Id,
                    paymentMethod.Card.Brand,
                    (int)paymentMethod.Card.ExpMonth,
                    (int)paymentMethod.Card.ExpYear,
                    paymentMethod.Card.Last4,
                    DateTime.Now,
                    false
                );

                await MPagoWriteRepository.AgregarMPago(mPago);

                return mPago.IdMPago.Valor;
            }
            catch (Exception ex)
            {
                throw new AgregarMPagoCommandHandlerException(ex);
            }
        }
    }
}
