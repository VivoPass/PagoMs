using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Application.DTOs
{
    public class AgregarMPagoDTO
    {
        public string IdUsuario { get; set; }
        public string IdMPagoStripe { get; set; }
        public string IdClienteStripe { get; set; }
        public string Marca { get; set; }
        public int MesExpiracion { get; set; }
        public int AnioExpiracion { get; set; }
        public string Ultimos4 { get; set; }
        public DateTime FechaRegistro { get; set; }
        public bool Predeterminado { get; set; }

    }
}
