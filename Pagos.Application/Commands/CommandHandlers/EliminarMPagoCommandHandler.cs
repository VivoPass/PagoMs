using MediatR;
using Pagos.Application.Interfaces;
using Pagos.Domain.Exceptions;
using Pagos.Domain.Interfaces;

namespace Pagos.Application.Commands.CommandHandlers
{
    public class EliminarMPagoCommandHandler : IRequestHandler<EliminarMPagoCommand, bool>
    {
        private readonly IMPagoRepository MPagoRepository;
        private readonly IStripeService StripeService;

        public EliminarMPagoCommandHandler(IMPagoRepository mPagoRepository, IStripeService stripeService)
        {
            MPagoRepository = mPagoRepository ?? throw new MPagoWriteRepositoryNullException();
            StripeService = stripeService ?? throw new StripeServiceNullException();
        }

        public async Task<bool> Handle(EliminarMPagoCommand idMpago, CancellationToken cancellationToken)
        {
            try
            {
                var mPago = await MPagoRepository.ObtenerMPagoPorId(idMpago.IdMPago);
                if (mPago == null)
                {
                    throw new MPagoNullException();
                }

                // Eliminar en Stripe
                await StripeService.EliminarMPago(mPago.IdClienteStripe.Valor, mPago.IdMPagoStripe.Valor);

                // Eliminar en Mongo
                await MPagoRepository.EliminarMPago(idMpago.IdMPago);

                return true;
            }
            catch (Exception ex)
            {
                throw new EliminarMPagoCommandHandlerException(ex);
            }
        }
    }
}
