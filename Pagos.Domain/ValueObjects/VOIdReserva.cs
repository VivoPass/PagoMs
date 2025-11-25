using Pagos.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Domain.ValueObjects
{
    public class VOIdReserva
    {
        public string Valor { get; private set; }
        public VOIdReserva(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new IdReservaNullException();

            if (!Guid.TryParse(valor, out _))
                throw new IdReservaInvalidoException();

            Valor = valor;
        }
    }
}
