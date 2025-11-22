using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Domain.ValueObjects
{
    public class VOPredeterminado
    {
        public bool Valor { get; private set; }
        public VOPredeterminado(bool valor)
        {
            Valor = valor;
        }
    }
}
