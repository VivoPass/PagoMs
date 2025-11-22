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
    public class GetTodosMPagoQueryHandler : IRequestHandler<GetTodosMPagoQuery, List<MPagoDTO>>
    {
        private readonly IMPagoRepository MPagoReadRepository;

        public GetTodosMPagoQueryHandler(IMPagoRepository mPagoReadRepository)
        {
            MPagoReadRepository = mPagoReadRepository ?? throw new MPagoReadRepositoryNullException();
        }

        public async Task<List<MPagoDTO>> Handle(GetTodosMPagoQuery todoslosmpago, CancellationToken cancellationToken)
        {
            try
            {
                var mpagos = await MPagoReadRepository.GetTodosMPago();

                if (mpagos == null || !mpagos.Any())
                {
                    throw new MPagoNullException();
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
                throw new GetTodosMPagoQueryHandlerException(ex);
            }
        }
    }
}
