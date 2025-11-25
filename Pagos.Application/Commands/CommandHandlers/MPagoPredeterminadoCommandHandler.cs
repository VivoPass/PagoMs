using MediatR;
using log4net;
using Pagos.Domain.Exceptions;
using Pagos.Domain.Interfaces;

namespace Pagos.Application.Commands.CommandHandlers
{
    public class MPagoPredeterminadoCommandHandler : IRequestHandler<MPagoPredeterminadoCommand, bool>
    {
        private readonly IMPagoRepository MPagoRepository;
        private readonly ILog Log;

        public MPagoPredeterminadoCommandHandler(IMPagoRepository mPagoRepository, ILog log)
        {
            MPagoRepository = mPagoRepository ?? throw new MPagoRepositoryNullException();
            Log = log ?? throw new LogNullException();
        }

        public async Task<bool> Handle(MPagoPredeterminadoCommand request, CancellationToken cancellationToken)
        {
            var idMpago = request.IdMPago;
            var idPostor = request.IdPostor;

            Log.Debug($"[INICIO] Manejando MPagoPredeterminadoCommand. MPago ID: {idMpago}, Postor ID: {idPostor}.");
            try
            {
                Log.Debug($"Paso 1: Verificando la existencia del MPago ID: {idMpago}.");
                var mPago = await MPagoRepository.ObtenerMPagoPorId(idMpago);
                if (mPago == null)
                {
                    Log.Error($"[VALIDACIÓN] No se encontró el MPago con ID {idMpago}.");
                    throw new MPagoNullException();
                }
                Log.Debug("Paso 1 OK: MPago verificado.");

                Log.Debug($"Paso 2: Obteniendo todos los MPagos del Postor ID: {idPostor}.");
                var MPagosPostor = await MPagoRepository.ObtenerMPagoPorIdUsuario(idPostor);
                if (MPagosPostor == null || !MPagosPostor.Any())
                {
                    Log.Error($"[VALIDACIÓN] No se encontraron MPagos para el Postor ID: {idPostor}.");
                    throw new MPagoIdUsuarioNullException();
                }

                var actualPredeterminado = MPagosPostor
                    .FirstOrDefault(mp => mp.Predeterminado.Valor && mp.IdMPago.Valor != idMpago);

                if (actualPredeterminado != null)
                {
                    var idActualPredeterminado = actualPredeterminado.IdMPago.Valor;
                    Log.Debug($"Paso 3: Se encontró un MPago predeterminado anterior ({idActualPredeterminado}). Desactivándolo.");
                    await MPagoRepository.ActualizarPredeterminadoFalseMPago(idActualPredeterminado);
                    Log.Info($"Paso 3 OK: MPago anterior predeterminado ({idActualPredeterminado}) cambiado a no predeterminado.");

                }
                else
                {
                    Log.Debug("Paso 3: No se encontró otro MPago predeterminado activo para el postor.");
                }

                Log.Debug($"Paso 4: Estableciendo el MPago ID: {idMpago} como predeterminado.");
                await MPagoRepository.ActualizarPredeterminadoTrueMPago(idMpago);
                Log.Info($"[ÉXITO] MPago ID {idMpago} establecido como predeterminado para el Postor ID {idPostor}.");

                return true;
            }
            catch (MPagoNullException)
            {
                throw;
            }
            catch (MPagoIdUsuarioNullException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Fatal($"[ERROR FATAL] Error no controlado al intentar establecer MPago ID {idMpago} como predeterminado para el Postor {idPostor}.", ex);
                throw new MPagoPredeterminadoCommandHandlerException(ex);
            }
        }
    }
}
