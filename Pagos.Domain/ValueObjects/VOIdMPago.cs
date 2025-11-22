using Pagos.Domain.Exceptions;

namespace Pagos.Domain.ValueObjects
{
    public class VOIdMPago
    {
        public string Valor { get; private set; }
        public VOIdMPago(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new IdMPagoNullException();

            if (!Guid.TryParse(valor, out _))
                throw new IdMPagoInvalidoException();

            Valor = valor;
        }
    }
}
