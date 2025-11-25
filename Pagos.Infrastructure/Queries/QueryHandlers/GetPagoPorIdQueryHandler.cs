using log4net;
using MediatR;
using Pagos.Application.DTOs;
using Pagos.Domain.Exceptions;
using Pagos.Domain.Interfaces;

namespace Pagos.Infrastructure.Queries.QueryHandlers
{
    public class GetPagoPorIdQueryHandler : IRequestHandler<GetPagoPorIdQuery, PagoDTO?>
    {
        private readonly IPagoRepository PagoRepository;
        private readonly ILog Log;

        public GetPagoPorIdQueryHandler(IPagoRepository pagoRepository, ILog log)
        {
            PagoRepository = pagoRepository ?? throw new PagoRepositoryNullException();
            Log = log ?? throw new LogNullException();
        }

        public async Task<PagoDTO?> Handle(GetPagoPorIdQuery idPago, CancellationToken cancellationToken)
        {
            var pagoId = idPago.IdPago;
            Log.Debug($"[ID: {pagoId}] Iniciando la consulta de pago por ID.");
            try
            {
                var pago = await PagoRepository.ObtenerPagoPorIdPago(idPago.IdPago);
                if (pago == null)
                {
                    Log.Warn($"[ID: {pagoId}] Pago no encontrado en el repositorio. Retornando null.");
                    return null;
                }

                Log.Debug($"[ID: {pagoId}] Pago encontrado. Procediendo al mapeo a DTO.");
                var pagoPorId = new PagoDTO
                {
                    IdPago = pago.IdPago.Valor,
                    IdMPago = pago.IdMPago.Valor,
                    IdUsuario = pago.IdUsuario.Valor,
                    IdReserva = pago.IdReserva.Valor,
                    IdEvento = pago.IdEvento.Valor,
                    FechaPago = pago.FechaPago.Valor,
                    Monto = pago.Monto.Valor,
                    IdExternalPago = pago.IdExternalPago.Valor ?? null
                };

                Log.Debug($"[ID: {pagoId}] Pago mapeado y retornado exitosamente.");
                return pagoPorId;
            }
            catch (Exception ex)
            {
                Log.Error($"[ID: {pagoId}] Error inesperado al obtener el pago. Lanzando GetPagoPorIdQueryHandlerException.", ex);
                throw new GetPagoPorIdQueryHandlerException(ex);
            }
        }
    }
}
