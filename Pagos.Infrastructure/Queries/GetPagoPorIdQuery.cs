using MediatR;
using Pagos.Application.DTOs;

namespace Pagos.Infrastructure.Queries
{
    public class GetPagoPorIdQuery : IRequest<PagoDTO?>
    {
        public string IdPago { get; set; }
        public GetPagoPorIdQuery(string idPago)
        {
            IdPago = idPago;
        }
    }
}
