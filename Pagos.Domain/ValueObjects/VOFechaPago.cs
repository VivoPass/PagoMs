using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Domain.ValueObjects
{
    public class VOFechaPago
    {
        public DateTime Valor { get; private set; }

        public VOFechaPago(DateTime valor)
        {
            Valor = valor;
        }

    }
}
