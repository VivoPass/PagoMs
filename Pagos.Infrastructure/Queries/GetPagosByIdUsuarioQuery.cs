using MediatR;
using Pagos.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Infrastructure.Queries
{
    public class GetPagosByIdUsuarioQuery : IRequest<List<PagoDTO>>
    {
        public string IdUsuario { get; set; }
        public GetPagosByIdUsuarioQuery(string idUsuario)
        {
            IdUsuario = idUsuario;
        }
    }
}
