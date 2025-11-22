using MediatR;
using Pagos.Application.DTOs;

namespace Pagos.Infrastructure.Queries
{
    public class GetMPagoPorIdQuery : IRequest<MPagoDTO>
    {
        public string IdMPago { get; set; }
        public GetMPagoPorIdQuery(string idMPago)
        {
            IdMPago = idMPago;
        }
    }
}
