using Pagos.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Infrastructure.Interfaces
{
    public interface IAuditoriaRepository
    {
        Task InsertarAuditoriaPago(string idUsuario, string level, string tipo, string idPago, string idMPago, decimal monto, string idReserva, string mensaje);
        Task InsertarAuditoriaMPago(string idUsuario, string level, string tipo, string idMPago, string mensaje);
    }
}
