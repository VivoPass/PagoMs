using Pagos.Domain.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Domain.Interfaces
{
    public interface IPagoRepository
    {
        Task AgregarPago(Pago pago);
        Task<Pago?> ObtenerPagoPorIdPago(string idPago);
        Task ActualizarIdPagoExterno(string idPago, string idExternalPago);
        Task<List<Pago>> ObtenerPagosPorIdUsuario(string idUsuario);
        Task<List<Pago>> ObtenerPagosPorIdEvento(string idEvento);
    }
}
