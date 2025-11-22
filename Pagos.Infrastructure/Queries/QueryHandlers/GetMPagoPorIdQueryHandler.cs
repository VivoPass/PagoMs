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
    public class GetMPagoPorIdQueryHandler : IRequestHandler<GetMPagoPorIdQuery, MPagoDTO>
    {
        private readonly IMPagoRepository MPagoReadRepository;

        public GetMPagoPorIdQueryHandler(IMPagoRepository mPagoReadRepository)
        {
            MPagoReadRepository = mPagoReadRepository ?? throw new MPagoReadRepositoryNullException();
        }

        public async Task<MPagoDTO> Handle(GetMPagoPorIdQuery idMPago, CancellationToken cancellationToken)
        {
            try
            {
                var mpago = await MPagoReadRepository.ObtenerMPagoPorId(idMPago.IdMPago);

                if (mpago == null)
                {
                    return new MPagoDTO();
                }

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

                return mpagoPorId;
            }
            catch (Exception ex)
            {
                throw new GetMPagoPorIdQueryHandlerException(ex);
            }
        }
    }
}
