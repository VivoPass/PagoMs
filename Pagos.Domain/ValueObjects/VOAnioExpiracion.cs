using Pagos.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Domain.ValueObjects
{
    public class VOAnioExpiracion
    {
        public int Valor { get; private set; }
        public VOAnioExpiracion(int valor)
        {
            if (valor < DateTime.UtcNow.Year)
                throw new AnioExpiracionInvalidoException();

            Valor = valor;
        }

    }
}
