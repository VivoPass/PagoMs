using Pagos.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Domain.ValueObjects
{
    public class VOIdUsuario
    {
        public string Valor { get; private set; }
        public VOIdUsuario(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new IdUsuarioNullException();

            if (!Guid.TryParse(valor, out _))
                throw new IdUsuarioInvalidoException();

            Valor = valor;
        }
    }
}
