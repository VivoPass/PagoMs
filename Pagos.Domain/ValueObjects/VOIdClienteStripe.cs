using Pagos.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Domain.ValueObjects
{
    public class VOIdClienteStripe
    {
        public string Valor { get; private set; }
        public VOIdClienteStripe(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new IdClienteStripeNullException();

            if (!valor.StartsWith("cus_"))
                throw new IdClienteStripeInvalidoException();

            Valor = valor;
        }
    }
}
