using MediatR;
using Pagos.Application.Interfaces;
using Pagos.Domain.Exceptions;
using Pagos.Domain.Interfaces;
using log4net;
using Stripe;

namespace Pagos.Application.Commands.CommandHandlers
{
    public class AgregarMPagoCommandHandler : IRequestHandler<AgregarMPagoCommand, string>
    {
        private readonly IMPagoRepository MPagoWriteRepository;
        private readonly IStripeService StripeService;
        private readonly ITarjetaCreditoFactory TarjetaCreditoFactory;
        private readonly ILog Log;
        private readonly IPaymentMethodService PaymentMethodService;

        public AgregarMPagoCommandHandler(IMPagoRepository mPagoWriteRepository, IStripeService stripeService, ITarjetaCreditoFactory tarjetaCreditoFactory,
            ILog log, IPaymentMethodService paymentMethodService)
        {
            MPagoWriteRepository = mPagoWriteRepository ?? throw new MPagoRepositoryNullException();
            StripeService = stripeService ?? throw new StripeServiceNullException();
            TarjetaCreditoFactory = tarjetaCreditoFactory ?? throw new TarjetaCreditoFactoryNullException();
            Log = log ?? throw new LogNullException();
            PaymentMethodService = paymentMethodService;
        }

        public async Task<string> Handle(AgregarMPagoCommand request, CancellationToken cancellationToken)
        {
            var idUsuario = request.MPagoStripe.IdUsuario;
            Log.Debug($"[INICIO] Manejando AgregarMPagoCommand para Usuario ID: {idUsuario}.");
            try
            {
                var mPagoStripeDto = request.MPagoStripe;
                if (string.IsNullOrWhiteSpace(mPagoStripeDto.IdMPagoStripe))
                {
                    Log.Error($"[VALIDACIÓN] IdMPagoStripe es nulo o vacío para el usuario {idUsuario}.");
                    throw new IdMPagoStripeNullCommandHandlerException();
                }
                Log.Debug($"Paso 1: Iniciando Creación de Cliente/Asociación MPago en Stripe. Email: {mPagoStripeDto.CorreoUsuario}.");
                // Crear cliente en Stripe y asociar el método de pago
                var customer = await StripeService.CrearTokenCUS(mPagoStripeDto.CorreoUsuario, mPagoStripeDto.IdMPagoStripe);
                Log.Info($"Paso 1 OK: Cliente Stripe ID {customer.Id} asociado/creado.");

                // Obtener detalles del método de pago desde Stripe
                var paymentMethod = await PaymentMethodService.GetAsync(mPagoStripeDto.IdMPagoStripe);
                Log.Debug($"Paso 2 OK: Detalles del MPago obtenidos de Stripe. Últimos 4 dígitos:" +
                          $" {paymentMethod.Card.Last4}, Marca: {paymentMethod.Card.Brand}.");

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
                Log.Debug($"Paso 3 OK: Entidad MPago local creada. ID local generado: {mPago.IdMPago.Valor}.");

                await MPagoWriteRepository.AgregarMPago(mPago);
                Log.Info($"[ÉXITO] MPago ID {mPago.IdMPago.Valor} guardado en la DB local para Usuario {idUsuario}.");

                return mPago.IdMPago.Valor;
            }
            catch (StripeException ex)
            {
                Log.Error($"[ERROR STRIPE] Fallo al procesar la solicitud de Stripe para el usuario {idUsuario}. Mensaje: {ex.Message}.", ex);
                throw new AgregarMPagoCommandHandlerException(ex);
            }
            catch (IdMPagoStripeNullCommandHandlerException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Fatal($"[ERROR FATAL] Error no controlado al agregar MPago para el usuario {idUsuario}.", ex);
                throw new AgregarMPagoCommandHandlerException(ex);
            }
        }
    }
}
