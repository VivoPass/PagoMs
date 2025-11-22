using MediatR;
using Pagos.Application.DTOs;
using Pagos.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pagos.Domain.Interfaces;

namespace Pagos.Infrastructure.Queries.QueryHandlers
{
    public class GetMPagoPorIdUsuarioQueryHandler : IRequestHandler<GetMPagoPorIdUsuarioQuery, List<MPagoDTO>>
    {
        private readonly IMPagoRepository MPagoReadRepository;

        public GetMPagoPorIdUsuarioQueryHandler(IMPagoRepository mPagoReadRepository)
        {
            MPagoReadRepository = mPagoReadRepository ?? throw new MPagoReadRepositoryNullException();
        }

        public async Task<List<MPagoDTO>> Handle(GetMPagoPorIdUsuarioQuery IdUsuario, CancellationToken cancellationToken)
        {
            try
            {
                var mpagos = await MPagoReadRepository.ObtenerMPagoPorIdUsuario(IdUsuario.IdUsuario);

                if (mpagos == null || !mpagos.Any())
                {
                    return new List<MPagoDTO>();
                }

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

                return listaMPagos;
            }
            catch (Exception ex)
            {
                throw new GetMPagoPorIdPostorQueryHandlerException(ex);
            }
        }
    }
}
