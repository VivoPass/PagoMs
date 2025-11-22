using MediatR;
using Pagos.Domain.Exceptions;
using Pagos.Domain.Interfaces;

namespace Pagos.Application.Commands.CommandHandlers
{
    public class MPagoPredeterminadoCommandHandler : IRequestHandler<MPagoPredeterminadoCommand, bool>
    {
        private readonly IMPagoRepository MPagoRepository;

        public MPagoPredeterminadoCommandHandler(IMPagoRepository mPagoRepository)
        {
            MPagoRepository = mPagoRepository ?? throw new MPagoWriteRepositoryNullException();
        }

        public async Task<bool> Handle(MPagoPredeterminadoCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar si el MPago existe
                var mPago = await MPagoRepository.ObtenerMPagoPorId(request.IdMPago);
                if (mPago == null)
                {
                    throw new MPagoNullException();
                }

                var MPagosPostor = await MPagoRepository.ObtenerMPagoPorIdUsuario(request.IdPostor);
                if (MPagosPostor == null || !MPagosPostor.Any())
                {
                    throw new MPagoIdUsuarioNullException();
                }

                var actualPredeterminado = MPagosPostor
                    .FirstOrDefault(mp => mp.Predeterminado.Valor && mp.IdMPago.Valor != request.IdMPago);

                if (actualPredeterminado != null)
                {
                    // Cambiar el MPago actual predeterminado a no predeterminado
                    await MPagoRepository.ActualizarPredeterminadoFalseMPago(actualPredeterminado.IdMPago.Valor);

                }

                // Establecer el MPago como predeterminado para el postor
                await MPagoRepository.ActualizarPredeterminadoTrueMPago(request.IdMPago);

                return true;
            }
            catch (Exception ex)
            {
                throw new MPagoPredeterminadoCommandHandlerException(ex);
            }
        }
    }
}
