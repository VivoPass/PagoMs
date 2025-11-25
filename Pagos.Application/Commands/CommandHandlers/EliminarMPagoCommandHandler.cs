using MediatR;
using Pagos.Application.Interfaces;
using Pagos.Domain.Exceptions;
using Pagos.Domain.Interfaces;
using log4net;

namespace Pagos.Application.Commands.CommandHandlers
{
    public class EliminarMPagoCommandHandler : IRequestHandler<EliminarMPagoCommand, bool>
    {
        private readonly IMPagoRepository MPagoRepository;
        private readonly IStripeService StripeService;
        private readonly ILog Log;

        public EliminarMPagoCommandHandler(IMPagoRepository mPagoRepository, IStripeService stripeService, ILog log)
        {
            MPagoRepository = mPagoRepository ?? throw new MPagoRepositoryNullException();
            StripeService = stripeService ?? throw new StripeServiceNullException();
            Log = log ?? throw new LogNullException();
        }

        public async Task<bool> Handle(EliminarMPagoCommand idMpago, CancellationToken cancellationToken)
        {
            var idMpagoLocal = idMpago.IdMPago;
            Log.Debug($"[INICIO] Manejando EliminarMPagoCommand para ID local: {idMpagoLocal}.");
            try
            {
                Log.Debug($"Paso 1: Buscando MPago con ID {idMpagoLocal} en la base de datos local.");
                var mPago = await MPagoRepository.ObtenerMPagoPorId(idMpago.IdMPago);
                if (mPago == null)
                {
                    Log.Error($"[VALIDACIÓN] No se encontró el MPago con ID {idMpagoLocal}.");
                    throw new MPagoNullException();
                }
                var idClienteStripe = mPago.IdClienteStripe.Valor;
                var idMpagoStripe = mPago.IdMPagoStripe.Valor;
                Log.Info($"Paso 1 OK: MPago encontrado. Cliente Stripe ID: {idClienteStripe}, MPago Stripe ID: {idMpagoStripe}.");


                // Eliminar en Stripe
                Log.Debug($"Paso 2: Eliminando MPago en Stripe. Cliente ID: {idClienteStripe}, MPago ID: {idMpagoStripe}.");
                await StripeService.EliminarMPago(idClienteStripe, idMpagoStripe);
                Log.Info("Paso 2 OK: Método de pago eliminado exitosamente de Stripe.");


                // Eliminar en Mongo
                Log.Debug($"Paso 3: Eliminando MPago local con ID {idMpagoLocal} de la DB.");
                await MPagoRepository.EliminarMPago(idMpagoLocal);
                Log.Info($"[ÉXITO] MPago ID {idMpagoLocal} eliminado exitosamente de la DB local.");


                return true;
            }
            catch (MPagoNullException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Fatal($"[ERROR FATAL] Error no controlado al eliminar MPago ID {idMpagoLocal}.", ex);
                throw new EliminarMPagoCommandHandlerException(ex);
            }
        }
    }
}
