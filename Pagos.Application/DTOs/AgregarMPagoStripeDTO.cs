using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Application.DTOs
{
    public class AgregarMPagoStripeDTO
    {
        public string IdUsuario { get; set; }
        public string IdMPagoStripe { get; set; }
        public string CorreoUsuario { get; set; }
    }
}
