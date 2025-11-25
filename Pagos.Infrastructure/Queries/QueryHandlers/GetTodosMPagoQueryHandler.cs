using log4net;
using MediatR;
using Pagos.Application.DTOs;
using Pagos.Domain.Exceptions;
using Pagos.Domain.Interfaces;

namespace Pagos.Infrastructure.Queries.QueryHandlers
{
    public class GetTodosMPagoQueryHandler : IRequestHandler<GetTodosMPagoQuery, List<MPagoDTO>>
    {
        private readonly IMPagoRepository MPagoReadRepository;
        private readonly ILog Log;

        public GetTodosMPagoQueryHandler(IMPagoRepository mPagoReadRepository, ILog log)
        {
            MPagoReadRepository = mPagoReadRepository ?? throw new MPagoRepositoryNullException();
            Log = log ?? throw new LogNullException();
        }

        public async Task<List<MPagoDTO>> Handle(GetTodosMPagoQuery todoslosmpago, CancellationToken cancellationToken)
        {
            Log.Debug("Iniciando la consulta para obtener TODOS los métodos de pago (Query de administración/lectura).");
            try
            {
                Log.Debug("Obteniendo todos los métodos de pago desde el repositorio.");
                var mpagos = await MPagoReadRepository.GetTodosMPago();

                if (mpagos == null || !mpagos.Any())
                {
                    Log.Fatal("El repositorio retornó una lista de métodos de pago nula o vacía. Lanzando MPagoNullException.");
                    throw new MPagoNullException();
                }

                Log.Info($"Se recuperaron {mpagos.Count} métodos de pago. Procediendo al mapeo a DTO.");
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

                Log.Debug($"Mapeo de {listaMPagos.Count} DTOs completado y retornado exitosamente.");
                return listaMPagos;
            }
            catch (MPagoNullException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Error("Error inesperado al obtener todos los métodos de pago. Lanzando excepción de Handler.", ex);
                throw new GetTodosMPagoQueryHandlerException(ex);
            }
        }
    }
}
