using log4net;
using MediatR;
using Pagos.Application.DTOs;
using Pagos.Domain.Exceptions;
using Pagos.Domain.Interfaces;

namespace Pagos.Infrastructure.Queries.QueryHandlers
{
    public class GetMPagoPorIdUsuarioQueryHandler : IRequestHandler<GetMPagoPorIdUsuarioQuery, List<MPagoDTO>>
    {
        private readonly IMPagoRepository MPagoReadRepository;
        private readonly ILog Log;

        public GetMPagoPorIdUsuarioQueryHandler(IMPagoRepository mPagoReadRepository, ILog log)
        {
            MPagoReadRepository = mPagoReadRepository ?? throw new MPagoRepositoryNullException();
            Log = log ?? throw new LogNullException();
        }

        public async Task<List<MPagoDTO>> Handle(GetMPagoPorIdUsuarioQuery IdUsuario, CancellationToken cancellationToken)
        {
            var usuarioId = IdUsuario.IdUsuario;

            Log.Debug($"[USUARIO ID: {usuarioId}] Iniciando la consulta de métodos de pago.");
            try
            {
                var mpagos = await MPagoReadRepository.ObtenerMPagoPorIdUsuario(usuarioId);

                if (mpagos == null || !mpagos.Any())
                {
                    Log.Warn($"[USUARIO ID: {usuarioId}] No se encontraron métodos de pago asociados al usuario. Retornando lista vacía.");
                    return new List<MPagoDTO>();
                }

                Log.Info($"[USUARIO ID: {usuarioId}] Se recuperaron {mpagos.Count} métodos de pago del repositorio. Procediendo al mapeo.");
                var listaMPagos = mpagos.Select(mp => new MPagoDTO
                {
                    IdMPago = mp.IdMPago.Valor,
                    IdUsuario = mp.IdUsuario.Valor,
                    IdMPagoStripe = mp.IdMPagoStripe.Valor,
                    IdClienteStripe = mp.IdClienteStripe.Valor,
                    Marca = mp.Marca.Valor,
                    MesExpiracion = mp.MesExpiracion.Valor,
                    AnioExpiracion = mp.AnioExpiracion.Valor,
                    Ultimos4 = mp.Ultimos4.Valor,
                    FechaRegistro = mp.FechaRegistro.Valor,
                    Predeterminado = mp.Predeterminado.Valor
                }).ToList();

                Log.Debug($"[USUARIO ID: {usuarioId}] Mapeo de {listaMPagos.Count} métodos de pago completado y retornado exitosamente.");
                return listaMPagos;
            }
            catch (Exception ex)
            {
                Log.Error($"[USUARIO ID: {usuarioId}] Error inesperado al obtener los métodos de pago. Lanzando excepción de Handler.", ex);
                throw new GetMPagoPorIdPostorQueryHandlerException(ex);
            }
        }
    }
}
