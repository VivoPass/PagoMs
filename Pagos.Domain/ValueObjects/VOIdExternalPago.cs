using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Domain.ValueObjects
{
    public class VOIdExternalPago
    {
        public string Valor { get; private set; }
        public VOIdExternalPago(string valor)
        {
            Valor = valor;
        }
    }
}
