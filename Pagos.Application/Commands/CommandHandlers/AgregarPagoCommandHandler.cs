using MediatR;
using Pagos.Application.Interfaces;
using Pagos.Domain.Exceptions;
using Pagos.Domain.Interfaces;
using log4net;

namespace Pagos.Application.Commands.CommandHandlers
{
    public class AgregarPagoCommandHandler : IRequestHandler<AgregarPagoCommand, string>
    {
        private readonly IPagoRepository PagoWriteRepository;
        private readonly IPagoFactory PagoFactory;
        private readonly IStripeService PaymentService;
        private readonly ILog Log;

        public AgregarPagoCommandHandler(IPagoRepository pagoWriteRepository, IPagoFactory pagoFactory, IStripeService paymentService, ILog log)
        {
            PagoWriteRepository = pagoWriteRepository ?? throw new PagoRepositoryNullException();
            PagoFactory = pagoFactory ?? throw new PagoFactoryNullException();
            PaymentService = paymentService ?? throw new StripeServiceNullException();
            Log = log ?? throw new LogNullException();
        }

        public async Task<string> Handle(AgregarPagoCommand Pago, CancellationToken cancellationToken)
        {
            var idUsuario = Pago.Pago.IdUsuario;
            var idReserva = Pago.Pago.IdReserva;
            var monto = Pago.Pago.Monto;
            Log.Debug($"[INICIO] Manejando AgregarPagoCommand. Usuario: {idUsuario}, Reserva: {idReserva}, Monto: {monto}.");
            try
            {
                var pago = PagoFactory.Crear(
                    Pago.Pago.IdMPago,
                    Pago.Pago.IdUsuario,
                    Pago.Pago.FechaPago,
                    Pago.Pago.Monto,
                    Pago.Pago.IdReserva,
                    Pago.Pago.IdEvento,
                    null
                );
                Log.Debug($"Paso 1: Entidad Pago local creada (ID local: {pago.IdPago.Valor}). Persistiendo registro preliminar...");
                await PagoWriteRepository.AgregarPago(pago);
                Log.Info($"Paso 1 OK: Pago preliminar guardado en DB local. ID: {pago.IdPago.Valor}.");

                Log.Debug($"Paso 2: Iniciando RealizarPago en PaymentService. Cliente ID: {Pago.IdUsuario}, MPago ID: {Pago.IdMPago}.");
                var paymentResult = await PaymentService.RealizarPago(
                    Pago.Pago.Monto,
                    Pago.IdUsuario,
                    Pago.IdMPago
                );
                if (paymentResult == null)
                {
                    //await PagoWriteRepository.EliminarPago(pago.IdPago.Valor);
                    Log.Error($"[ERROR PAGO EXTERNO] El servicio de pago (Stripe) devolvió un resultado nulo. Pago fallido para Reserva {idReserva}.");
                    throw new PagoUnsuccessfulException();
                }
                Log.Info($"Paso 2 OK: Pago exitoso. ID externo (Stripe): {paymentResult}.");

                Log.Debug($"Paso 3: Actualizando registro local con ID de Pago Externo ({paymentResult}).");
                await PagoWriteRepository.ActualizarIdPagoExterno(pago.IdPago.Valor, paymentResult);
                Log.Info($"Paso 3 OK: Pago ID local {pago.IdPago.Valor} completado y actualizado.");

                return pago.IdPago.Valor;
            }
            catch (PagoUnsuccessfulException)
            {
                Log.Warn($"[FALLO TRANSACCIÓN] Se detectó una excepción de pago fallido para la Reserva {idReserva}.");
                throw;
            }
            catch (Exception ex)
            {
                Log.Fatal($"[ERROR FATAL] Error no controlado al procesar el pago para la Reserva {idReserva}.", ex);
                throw new AgregarPagoCommandHandlerException(ex);
            }
        }
    }
}
