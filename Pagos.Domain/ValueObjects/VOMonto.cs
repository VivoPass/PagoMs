using Pagos.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Domain.ValueObjects
{
    public class VOMonto
    {
        public decimal Valor { get; private set; }

        public VOMonto(decimal valor)
        {
            if (valor <= 0)
                throw new MontoInvalido();

            Valor = valor;
        }

    }
}
