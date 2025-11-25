using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pagos.Domain.Exceptions;

namespace Pagos.Domain.ValueObjects
{
    public class VOIdPago
    {
        public string Valor { get; private set; }
        public VOIdPago(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new IdPagoNullException();

            if (!Guid.TryParse(valor, out _))
                throw new IdPagoInvalidoException();

            Valor = valor;
        }
    }
}
