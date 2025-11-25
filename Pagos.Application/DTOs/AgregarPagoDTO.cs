using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Application.DTOs
{
    public class AgregarPagoDTO
    {
        public string IdMPago { get; set; }
        public string IdUsuario { get; set; }
        public string IdReserva { get; set; }
        public string IdEvento { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal Monto { get; set; }
    }
}
