using Pagos.Domain.Entities;
using Pagos.Domain.Interfaces;
using Pagos.Domain.ValueObjects;

namespace Pagos.Domain.Factories
{
    public class TarjetaCreditoFactory : ITarjetaCreditoFactory
    {
        public TarjetaCredito Crear
        (string idUsuario, string idMPagoStripe, string idClienteStripe, string marca, int mesExpiracion,
            int anioExpiracion, string ultimos4, DateTime fechaRegistro, bool predeterminado)
        {
            VOIdMPago IdMPago = new VOIdMPago(Guid.NewGuid().ToString());
            VOIdUsuario IdUsuario = new VOIdUsuario(idUsuario);
            VOIdMPagoStripe IdMPagoStripe = new VOIdMPagoStripe(idMPagoStripe);
            VOIdClienteStripe IdClienteStripe = new VOIdClienteStripe(idClienteStripe);
            VOMarca Marca = new VOMarca(marca);
            VOMesExpiracion MesExpiracion = new VOMesExpiracion(mesExpiracion);
            VOAnioExpiracion AnioExpiracion = new VOAnioExpiracion(anioExpiracion);
            VOUltimos4 Ultimos4 = new VOUltimos4(ultimos4);
            VOFechaRegistro FechaRegistro = new VOFechaRegistro(fechaRegistro);
            VOPredeterminado Predeterminado = new VOPredeterminado(predeterminado);

            var NuevoMPago = new TarjetaCredito(IdMPago, IdUsuario, IdMPagoStripe, IdClienteStripe, Marca, MesExpiracion, AnioExpiracion, 
                Ultimos4, FechaRegistro, Predeterminado);

            return NuevoMPago;
        }

        public TarjetaCredito Load(VOIdMPago idMPago, VOIdUsuario idUsuario, VOIdMPagoStripe idMPagoStripe,
            VOIdClienteStripe idClienteStripe, VOMarca marca, VOMesExpiracion mesExpiracion,
            VOAnioExpiracion anioExpiracion, VOUltimos4 ultimos4, VOFechaRegistro fechaRegistro, VOPredeterminado predeterminado)
        {
            return new TarjetaCredito(idMPago, idUsuario, idMPagoStripe, idClienteStripe, marca, mesExpiracion, anioExpiracion, ultimos4,
                fechaRegistro, predeterminado);
        }
    }
}
