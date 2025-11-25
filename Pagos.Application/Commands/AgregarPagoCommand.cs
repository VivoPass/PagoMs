using MediatR;
using Pagos.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Application.Commands
{
    public class AgregarPagoCommand : IRequest<String>
    {
        public AgregarPagoDTO Pago { get; set; }
        public string IdUsuario { get; set; }
        public string IdMPago { get; set; }
        public AgregarPagoCommand(AgregarPagoDTO pago, string idUsuario, string idMPago)
        {
            Pago = pago;
            IdUsuario = idUsuario;
            IdMPago = idMPago;
        }
    }
}
