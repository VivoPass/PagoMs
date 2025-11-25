using MediatR;
using Pagos.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Infrastructure.Queries
{
    public class GetPagosByIdEventoQuery : IRequest<List<PagoDTO>>
    {
        public string IdEvento { get; set; }
        public GetPagosByIdEventoQuery(string idEvento)
        {
            IdEvento = idEvento;
        }
    }
}
