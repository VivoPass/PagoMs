using Pagos.Domain.Aggregates;
using Pagos.Domain.Entities;
using Pagos.Domain.Interfaces;
using Pagos.Domain.ValueObjects;

namespace Pagos.Domain.Factories
{
    public class PagoFactory : IPagoFactory
    {
        public Pago Crear
        (string idMPago, string idUsuario, DateTime fechaPago, decimal monto, string idReserva,
            string idExternalPago)
        {
            VOIdPago IdPago = new VOIdPago(Guid.NewGuid().ToString());
            VOIdMPago IdMPago = new VOIdMPago(idMPago);
            VOIdUsuario IdUsuario = new VOIdUsuario(idUsuario);
            VOFechaPago FechaPago = new VOFechaPago(fechaPago);
            VOMonto Monto = new VOMonto(monto);
            VOIdReserva IdReserva = new VOIdReserva(idReserva);
            VOIdExternalPago? IdExternalPago = new VOIdExternalPago(idExternalPago);

            var NuevoMPago = new Pago(IdPago, IdMPago, IdUsuario, FechaPago, Monto, IdReserva, IdExternalPago);

            return NuevoMPago;
        }

        public Pago Load(VOIdPago idPago, VOIdMPago idMPago, VOIdUsuario idUsuario, VOFechaPago fechaPago, VOMonto monto, VOIdReserva idReserva,
            VOIdExternalPago? idExternalPago)
        {
            return new Pago(idPago, idMPago, idUsuario, fechaPago, monto, idReserva, idExternalPago);
        }
    }
}
