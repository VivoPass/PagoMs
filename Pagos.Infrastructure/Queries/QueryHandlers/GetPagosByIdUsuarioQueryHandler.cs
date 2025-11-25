using log4net;
using MediatR;
using Pagos.Application.DTOs;
using Pagos.Domain.Exceptions;
using Pagos.Domain.Interfaces;

namespace Pagos.Infrastructure.Queries.QueryHandlers
{
    public class GetPagosByIdUsuarioQueryHandler : IRequestHandler<GetPagosByIdUsuarioQuery, List<PagoDTO>>
    {
        private readonly IPagoRepository PagoRepository;
        private readonly ILog Log;

        public GetPagosByIdUsuarioQueryHandler(IPagoRepository pagoRepository, ILog log)
        {
            PagoRepository = pagoRepository ?? throw new PagoRepositoryNullException();
            Log = log ?? throw new LogNullException();
        }

        public async Task<List<PagoDTO>> Handle(GetPagosByIdUsuarioQuery pagoIdUsuario, CancellationToken cancellationToken)
        {
            var usuarioId = pagoIdUsuario.IdUsuario;
            Log.Debug($"[USUARIO ID: {usuarioId}] Iniciando la consulta de todos los pagos realizados por el usuario.");
            try
            {
                var pagos = await PagoRepository.ObtenerPagosPorIdUsuario(usuarioId);
                if (pagos == null || !pagos.Any())
                {
                    Log.Info($"[USUARIO ID: {usuarioId}] No se encontraron pagos para el usuario. Retornando lista vacía.");
                    return new List<PagoDTO>();
                }

                Log.Info($"[USUARIO ID: {usuarioId}] Se recuperaron {pagos.Count} registros de pago. Procediendo al mapeo a DTO.");
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

                Log.Debug($"[USUARIO ID: {usuarioId}] Mapeo de {Pagos.Count} Pagos a DTO completado y retornado exitosamente.");
                return Pagos;
            }
            catch (Exception ex)
            {
                Log.Error($"[USUARIO ID: {usuarioId}] Error inesperado al obtener la lista de pagos. Lanzando excepción de Handler.", ex);
                throw new GetPagosUsuarioQueryHandlerException(ex);
            }
        }
    }
}
