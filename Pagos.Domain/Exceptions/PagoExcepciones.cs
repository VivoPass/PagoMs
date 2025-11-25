using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Domain.Exceptions
{
    /*/////////////////APLICACION/////////////////*/
    #region COMMANDS EXCEPTIONS
    //AgregarPagoCommandHandler
    public class PagoUnsuccessfulException : Exception
    {
        public PagoUnsuccessfulException() : base("Error al procesar el pago.") { }
    }
    public class AgregarPagoCommandHandlerException : Exception
    {
        public AgregarPagoCommandHandlerException(Exception inner) : base("No fue posible agregar el Pago al dominio. " +
                                                                          "El comando no cumplió con las reglas de negocio definidas.", inner)
        { }
    }
    #endregion

    /*/////////////////DOMINIO/////////////////*/
    #region VALUE OBJECTS EXCEPTIONS
    //VOIdPago
    public class IdPagoNullException : Exception
    {
        public IdPagoNullException() : base("El ID del pago no puede estar vacío.") { }
    }
    public class IdPagoInvalidoException : Exception
    {
        public IdPagoInvalidoException() : base("El ID del pago debe ser un GUID válido.") { }
    }

    //VOIdReserva
    public class IdReservaNullException : Exception
    {
        public IdReservaNullException() : base("El ID de la reserva no puede estar vacío.") { }
    }
    public class IdReservaInvalidoException : Exception
    {
        public IdReservaInvalidoException() : base("El ID de la reserva debe ser un GUID válido.") { }
    }

    //VOIdEvento
    public class IdEventoNullException : Exception
    {
        public IdEventoNullException() : base("El ID del evento no puede estar vacío.") { }
    }
    public class IdEventoInvalidoException : Exception
    {
        public IdEventoInvalidoException() : base("El ID del evento debe ser un GUID válido.") { }
    }

    //VOMonto
    public class MontoInvalido : Exception
    {
        public MontoInvalido() : base("El monto del pago debe ser mayor a cero.") { }
    }
    #endregion

    #region FACTORIES EXCEPTIONS
    //PagoFactory
    public class PagoFactoryNullException : Exception
    {
        public PagoFactoryNullException() : base("El componente IPagoFactory no fue inicializado correctamente. " +
                                                 "Asegúrate de que esté registrado en el contenedor de dependencias.")
        { }
    }
    #endregion

    /*/////////////////INFRAESTRUCTURA/////////////////*/
    #region REPOSITORIES EXCEPTIONS
    //MPagoRepository
    public class PagoRepositoryNullException : Exception
    {
        public PagoRepositoryNullException() : base("El componente IPagoRepository no fue inicializado correctamente. " +
                                                    "Asegúrate de que esté registrado en el contenedor de dependencias.")
        { }
    }
    public class ErrorConexionBd : Exception
    {
        public ErrorConexionBd(Exception inner) : base("No se pudo conectar a la base de datos.", inner) { }
    }
    public class PagoRepositoryException : Exception
    {
        public PagoRepositoryException(Exception inner) : base("Fallo en PagoRepository. No se pudo completar la operación.", inner)
        { }
    }
    public class PagoNullRepositoryException : Exception
    {
        public PagoNullRepositoryException() : base("No se encontró el Pago que se esta buscando.")
        { }
    }
    #endregion

    #region QUERIES EXCEPTIONS
    //GetPagoPorIdQueryHandler
    public class GetPagoPorIdQueryHandlerException : Exception
    {
        public GetPagoPorIdQueryHandlerException(Exception inner)
            : base("El manejador de la consulta GetPagoPorId no pudo obtener la entidad Pago del repositorio.", inner)
        { }
    }

    //GetPagosByIdEventoQueryHandler
    public class GetPagosByIdEventoQueryHandlerException : Exception
    {
        public GetPagosByIdEventoQueryHandlerException(Exception inner)
            : base("El manejador de la consulta GetPagosByIdEvento no pudo obtener la entidad Pago del repositorio.", inner)
        { }
    }

    //GetPagosByIdUsuarioQueryHandler
    public class GetPagosUsuarioQueryHandlerException : Exception
    {
        public GetPagosUsuarioQueryHandlerException(Exception inner)
            : base("El manejador de la consulta GetPagosByIdUsuario no pudo obtener la entidad Pago del repositorio.", inner)
        { }
    }
    #endregion
}
