using Pagos.Domain.Exceptions;

namespace Pagos.Domain.ValueObjects
{
    public class VOIdEvento
    {
        public string Valor { get; private set; }
        public VOIdEvento(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new IdEventoNullException();

            if (!Guid.TryParse(valor, out _))
                throw new IdEventoInvalidoException();

            Valor = valor;
        }
    }
}
