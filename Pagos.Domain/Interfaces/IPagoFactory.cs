using Pagos.Domain.Aggregates;
using Pagos.Domain.ValueObjects;

namespace Pagos.Domain.Interfaces
{
    public interface IPagoFactory
    {
        Pago Crear
        (string idMPago, string idUsuario, DateTime fechaPago, decimal monto, string idReserva, string idEvento,
            string idExternalPago);
        Pago Load(VOIdPago idPago, VOIdMPago idMPago, VOIdUsuario idUsuario, VOFechaPago fechaPago, VOMonto monto, VOIdReserva idReserva, 
            VOIdEvento idEvento, VOIdExternalPago? idExternalPago);
    }
}