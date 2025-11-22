using Pagos.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Domain.ValueObjects
{
    public class VOUltimos4
    {
        public string Valor { get; private set; }

        public VOUltimos4(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new Ultimos4NullException();

            if (valor.Length < 4 || valor.Length > 4)
                throw new Ultimos4DigitosException();

            if (!valor.All(char.IsDigit))
                throw new Ultimos4NumericoException();

            Valor = valor;
        }
        public override string ToString() => $"**** **** **** {Valor}";
    }
}
