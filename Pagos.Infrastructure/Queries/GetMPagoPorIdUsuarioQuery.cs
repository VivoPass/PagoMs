using MediatR;
using Pagos.Application.DTOs;

namespace Pagos.Infrastructure.Queries
{
    public class GetMPagoPorIdUsuarioQuery : IRequest<List<MPagoDTO>>
    {
        public string IdUsuario { get; set; }
        public GetMPagoPorIdUsuarioQuery(string idUsuario)
        {
            IdUsuario = idUsuario;
        }
    }
}
