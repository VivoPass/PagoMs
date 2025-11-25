using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Domain.Exceptions
{
    /*/////////////////APLICACION/////////////////*/
    #region COMMANDS EXCEPTIONS
    //AgregarMPagoCommandHandler
    public class IdMPagoStripeNullCommandHandlerException : Exception
    {
        public IdMPagoStripeNullCommandHandlerException() : base("El IdMPagoStripe es requerido.")
        { }
    }
    public class AgregarMPagoCommandHandlerException : Exception
    {
        public AgregarMPagoCommandHandlerException(Exception inner) : base("No fue posible agregar el MPago al dominio. " +
                                                                           "El comando no cumplió con las reglas de negocio definidas.", inner)
        { }
    }

    //EliminarMPagoCommandHandler
    public class MPagoNullException : Exception
    {
        public MPagoNullException() : base("No se encontró el MPago que se esta buscando.")
        { }
    }
    public class EliminarMPagoCommandHandlerException : Exception
    {
        public EliminarMPagoCommandHandlerException(Exception inner) : base("No fue posible eliminar el MPago al dominio. " +
                                                                            "El comando no cumplió con las reglas de negocio definidas.", inner)
        { }
    }

    //MPagoPredeterminadoCommandHandler
    public class MPagoIdUsuarioNullException : Exception
    {
        public MPagoIdUsuarioNullException() : base("No se encontraron MPagos para el usuario especificado.")
        { }
    }
    public class MPagoPredeterminadoCommandHandlerException : Exception
    {
        public MPagoPredeterminadoCommandHandlerException(Exception inner) : base("No fue posible colocar como predeterminado el MPago al dominio. " +
            "El comando no cumplió con las reglas de negocio definidas.", inner)
        { }
    }
    #endregion

    /*/////////////////DOMINIO/////////////////*/
    #region VALUE OBJECTS EXCEPTIONS
    //VOIdMPago
    public class IdMPagoNullException : Exception
    {
        public IdMPagoNullException() : base("El ID del metodo de pago no puede estar vacío.") { }
    }
    public class IdMPagoInvalidoException : Exception
    {
        public IdMPagoInvalidoException() : base("El ID del metodo de pago debe ser un GUID válido.") { }
    }

    //VOIdUsuario
    public class IdUsuarioNullException : Exception
    {
        public IdUsuarioNullException() : base("El ID de usuario no puede estar vacío.") { }
    }
    public class IdUsuarioInvalidoException : Exception
    {
        public IdUsuarioInvalidoException() : base("El ID de usuario debe ser un GUID válido.") { }
    }

    //VOIdMPagoStripe
    public class IdMPagoStripeNullException : Exception
    {
        public IdMPagoStripeNullException() : base("El ID del método de pago no puede estar vacío.") { }
    }
    public class IdMPagoStripeInvalidoException : Exception
    {
        public IdMPagoStripeInvalidoException() : base("El ID del método de pago debe comenzar con 'pm_'.") { }
    }

    //VOIdClienteStripe
    public class IdClienteStripeNullException : Exception
    {
        public IdClienteStripeNullException() : base("El ID del cliente no puede estar vacío.") { }
    }
    public class IdClienteStripeInvalidoException : Exception
    {
        public IdClienteStripeInvalidoException() : base("El ID del cliente debe comenzar con 'cus_'.") { }
    }

    //VOMarca
    public class MarcaNullException : Exception
    {
        public MarcaNullException() : base("La marca de la tarjeta no puede estar vacía.") { }
    }
    public class MarcaIncompatibleException : Exception
    {
        public MarcaIncompatibleException(string marca) : base($"Marca de tarjeta no reconocida: '{marca}'.") { }
    }

    //VOMesExpiracion
    public class MesExpiracionDigitosException : Exception
    {
        public MesExpiracionDigitosException() : base("El mes de expiración debe estar entre 1 y 12.") { }
    }

    //VOAnioExpiracion
    public class AnioExpiracionInvalidoException : Exception
    {
        public AnioExpiracionInvalidoException() : base("El año de expiración debe ser mayor al actual.") { }
    }

    //VOUltimos4
    public class Ultimos4NullException : Exception
    {
        public Ultimos4NullException() : base("Los ultimos 4 digitos no puede estar vacíos.") { }
    }
    public class Ultimos4DigitosException : Exception
    {
        public Ultimos4DigitosException() : base("Los ultimos 4 digitos debe tener 4 dígitos.") { }
    }
    public class Ultimos4NumericoException : Exception
    {
        public Ultimos4NumericoException() : base("Los ultimos 4 digitos deben ser numéricos.") { }
    }

    //VOFechaRegistro
    public class FechaRegistroInvalidaException : Exception
    {
        public FechaRegistroInvalidaException() : base("La fecha de registro no puede ser en el futuro.") { }
    }

    #endregion

    #region FACTORIES EXCEPTIONS
    //TarjetaCreditoFactory
    public class TarjetaCreditoFactoryNullException : Exception
    {
        public TarjetaCreditoFactoryNullException() : base("El componente ITarjetaCreditoFactory no fue inicializado correctamente. " +
                                                           "Asegúrate de que esté registrado en el contenedor de dependencias.")
        { }
    }
    #endregion

    /*/////////////////INFRAESTRUCTURA/////////////////*/
    #region REPOSITORIES EXCEPTIONS
    //MPagoRepository
    public class MPagoRepositoryNullException : Exception
    {
        public MPagoRepositoryNullException() : base("El componente IMPagoRepository no fue inicializado correctamente. " +
                                                     "Asegúrate de que esté registrado en el contenedor de dependencias.")
        { }
    }
    public class MPagoNullRepositoryException : Exception
    {
        public MPagoNullRepositoryException() : base("No se encontró el MPago que se esta buscando.")
        { }
    }
    public class MPagoRepositoryException : Exception
    {
        public MPagoRepositoryException(Exception inner) : base("Fallo en MPagoRepository. No se pudo completar la operación.", inner)
        { }
    }
    public class MongoDBCommandException : Exception
    {
        public MongoDBCommandException(Exception inner) : base("Error al ejecutar el comando en la base de datos.", inner) { }
    }
    #endregion

    #region QUERIES EXCEPTIONS
    //GetMPagoPorIdQueryHandler
    public class GetMPagoPorIdQueryHandlerException : Exception
    {
        public GetMPagoPorIdQueryHandlerException(Exception inner)
            : base("El manejador de la consulta GetMPagoPorId no pudo obtener la entidad MPago del repositorio.", inner)
        { }
    }

    //GetMPagoPorIdUsuarioQueryHandler
    public class GetMPagoPorIdPostorQueryHandlerException : Exception
    {
        public GetMPagoPorIdPostorQueryHandlerException(Exception inner)
            : base("El manejador de la consulta GetMPagoPorIdPostor no pudo obtener la entidad MPago del repositorio.", inner)
        { }
    }

    //GetTodosMPagoQueryHandler
    public class GetTodosMPagoQueryHandlerException : Exception
    {
        public GetTodosMPagoQueryHandlerException(Exception inner)
            : base("El manejador de la consulta GetTodosMPago no pudo obtener la entidad MPago del repositorio.", inner)
        { }
    }
    #endregion
}
