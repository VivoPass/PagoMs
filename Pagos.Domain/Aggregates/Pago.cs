using Pagos.Domain.ValueObjects;

namespace Pagos.Domain.Aggregates
{
    public class Pago
    {
        public VOIdPago IdPago { get; private set; }
        public VOIdMPago IdMPago { get; private set; }
        public VOIdExternalPago? IdExternalPago { get; private set; }
        public VOIdUsuario IdUsuario { get; private set; }
        public VOIdReserva IdReserva { get; private set; }
        public VOFechaPago FechaPago { get; private set; }
        public VOMonto Monto { get; private set; }

        public Pago(VOIdPago idPago, VOIdMPago idMPago, VOIdUsuario idUsuario, VOFechaPago fechaPago, VOMonto monto, VOIdReserva idReserva, VOIdExternalPago? idExternalPago)
        {
            IdPago = idPago;
            IdMPago = idMPago;
            IdUsuario = idUsuario;
            FechaPago = fechaPago;
            Monto = monto;
            IdReserva = idReserva;
            IdExternalPago = idExternalPago;
        }
    }
}
