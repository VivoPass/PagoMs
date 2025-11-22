using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pagos.Domain.Exceptions;

namespace Pagos.Domain.ValueObjects
{
    public class VOMarca
    {
        private static readonly HashSet<string> MarcasValidas = new()
            {"visa", "mastercard", "american express", "discover", "jcb", "diners club", "unionpay"};
        public string Valor { get; private set; }
        public VOMarca(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new MarcaNullException();

            if (!MarcasValidas.Contains(valor.ToLower()))
                throw new MarcaIncompatibleException(valor);

            Valor = valor;
        }
    }
}
