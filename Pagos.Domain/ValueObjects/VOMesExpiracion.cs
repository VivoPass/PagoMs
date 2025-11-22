using Pagos.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Domain.ValueObjects
{
    public class VOMesExpiracion
    {
        public int Valor { get; private set; }
        public VOMesExpiracion(int valor)
        {
            if (valor < 1 || valor > 12)
                throw new MesExpiracionDigitosException();

            Valor = valor;
        }

    }
}
