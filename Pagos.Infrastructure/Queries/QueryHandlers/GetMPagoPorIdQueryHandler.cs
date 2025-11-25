using log4net;
using MediatR;
using Pagos.Application.DTOs;
using Pagos.Domain.Exceptions;
using Pagos.Domain.Interfaces;

namespace Pagos.Infrastructure.Queries.QueryHandlers
{
    public class GetMPagoPorIdQueryHandler : IRequestHandler<GetMPagoPorIdQuery, MPagoDTO>
    {
        private readonly IMPagoRepository MPagoReadRepository;
        private readonly ILog Log;

        public GetMPagoPorIdQueryHandler(IMPagoRepository mPagoReadRepository, ILog log)
        {
            MPagoReadRepository = mPagoReadRepository ?? throw new MPagoRepositoryNullException();
            Log = log ?? throw new LogNullException();
        }

        public async Task<MPagoDTO> Handle(GetMPagoPorIdQuery idMPago, CancellationToken cancellationToken)
        {
            var mpagoId = idMPago.IdMPago;
            Log.Debug($"[MPAGO ID: {mpagoId}] Iniciando la consulta para obtener método de pago por ID.");
            try
            {
                var mpago = await MPagoReadRepository.ObtenerMPagoPorId(mpagoId);

                if (mpago == null)
                {
                    Log.Warn($"[MPAGO ID: {mpagoId}] El método de pago no fue encontrado en el repositorio. Retornando DTO vacío.");
                    return new MPagoDTO();
                }

                Log.Info($"[MPAGO ID: {mpagoId}] Método de pago encontrado. Procediendo al mapeo.");
                var mpagoPorId = new MPagoDTO
                {
                    IdMPago = mpago.IdMPago.Valor,
                    IdUsuario = mpago.IdUsuario.Valor,
                    IdMPagoStripe = mpago.IdMPagoStripe.Valor,
                    IdClienteStripe = mpago.IdClienteStripe.Valor,
                    Marca = mpago.Marca.Valor,
                    MesExpiracion = mpago.MesExpiracion.Valor,
                    AnioExpiracion = mpago.AnioExpiracion.Valor,
                    Ultimos4 = mpago.Ultimos4.Valor,
                    FechaRegistro = mpago.FechaRegistro.Valor,
                    Predeterminado = mpago.Predeterminado.Valor
                };

                Log.Debug($"[MPAGO ID: {mpagoId}] Mapeo a DTO completado y retornado exitosamente.");
                return mpagoPorId;
            }
            catch (Exception ex)
            {
                Log.Error($"[MPAGO ID: {mpagoId}] Error inesperado al obtener el método de pago. Lanzando excepción de Handler.", ex);
                throw new GetMPagoPorIdQueryHandlerException(ex);
            }
        }
    }
}
