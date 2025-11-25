using log4net;
using MediatR;
using Pagos.Application.DTOs;
using Pagos.Domain.Exceptions;
using Pagos.Domain.Interfaces;

namespace Pagos.Infrastructure.Queries.QueryHandlers
{
    public class GetPagosByIdEventoQueryHandler : IRequestHandler<GetPagosByIdEventoQuery, List<PagoDTO>>
    {
        private readonly IPagoRepository PagoRepository;
        private readonly ILog Log;

        public GetPagosByIdEventoQueryHandler(IPagoRepository pagoRepository, ILog log)
        {
            PagoRepository = pagoRepository ?? throw new PagoRepositoryNullException();
            Log = log ?? throw new LogNullException();
        }

        public async Task<List<PagoDTO>> Handle(GetPagosByIdEventoQuery idEvento, CancellationToken cancellationToken)
        {
            var eventoId = idEvento.IdEvento;
            Log.Debug($"[EVENTO ID: {eventoId}] Iniciando la consulta de pagos por ID de Evento.");
            try
            {
                var pagos = await PagoRepository.ObtenerPagosPorIdEvento(eventoId);
                Log.Info($"[EVENTO ID: {eventoId}] Se recuperaron {pagos.Count} pagos del repositorio.");

                var Pagos = new List<PagoDTO>();
                foreach (var pago in pagos)
                {
                    var returnPago = new PagoDTO
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
                    Pagos.Add(returnPago);
                }

                Log.Debug($"[EVENTO ID: {eventoId}] Mapeo a DTO completado. Retornando {Pagos.Count} resultados.");
                return Pagos;
            }
            catch (Exception ex)
            {
                Log.Error($"[EVENTO ID: {eventoId}] Error inesperado al procesar la consulta. Lanzando GetPagosByIdEventoQueryHandlerException.", ex);
                throw new GetPagosByIdEventoQueryHandlerException(ex);
            }
        }
    }
}
