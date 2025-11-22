using MediatR;

namespace Pagos.Application.Commands
{
    public class MPagoPredeterminadoCommand : IRequest<bool>
    {
        public string IdMPago { get; set; }
        public string IdPostor { get; set; }
        public MPagoPredeterminadoCommand(string idMPago, string idPostor)
        {
            IdMPago = idMPago;
            IdPostor = idPostor;
        }
    }
}
