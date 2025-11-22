using Pagos.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Domain.ValueObjects
{
    public class VOIdMPagoStripe
    {
        public string Valor { get; private set; }
        public VOIdMPagoStripe(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new IdMPagoStripeNullException();

            if (!valor.StartsWith("pm_"))
                throw new IdMPagoStripeInvalidoException();

            Valor = valor;
        }

    }
}
