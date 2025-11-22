using Pagos.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Domain.ValueObjects
{
    public class VOFechaRegistro
    {
        public DateTime Valor { get; private set; }
        public VOFechaRegistro(DateTime valor)
        {
            if (valor > DateTime.UtcNow.AddMinutes(2))
                throw new FechaRegistroInvalidaException();

            Valor = valor;
        }
    }
}
