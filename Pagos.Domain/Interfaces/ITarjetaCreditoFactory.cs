using Pagos.Domain.Factories;
using Pagos.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pagos.Domain.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Pagos.Domain.Interfaces
{
    public interface ITarjetaCreditoFactory
    {
        TarjetaCredito Crear
            (string idUsuario, string idMPagoStripe, string idClienteStripe, string marca, int mesExpiracion, 
                int anioExpiracion, string ultimos4, DateTime fechaRegistro, bool predeterminado);
        TarjetaCredito Load(VOIdMPago idMPago, VOIdUsuario idUsuario, VOIdMPagoStripe idMPagoStripe,
            VOIdClienteStripe idClienteStripe, VOMarca marca, VOMesExpiracion mesExpiracion,
            VOAnioExpiracion anioExpiracion, VOUltimos4 ultimos4, VOFechaRegistro fechaRegistro, VOPredeterminado predeterminado);
    }
}