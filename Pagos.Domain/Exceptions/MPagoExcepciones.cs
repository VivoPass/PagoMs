using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Domain.Exceptions
{
    //VALUE OBJECTS EXCEPTIONS
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
    public class NumeroTarjetaNullException : Exception
    {
        public NumeroTarjetaNullException() : base("El número de tarjeta no puede estar vacío.") { }
    }
    public class NumeroTarjetaDigitosException : Exception
    {
        public NumeroTarjetaDigitosException() : base("El número de tarjeta debe tener entre 13 y 19 dígitos.") { }
    }
    public class NumeroTarjetaNumericoException : Exception
    {
        public NumeroTarjetaNumericoException() : base("El número de tarjeta debe contener solo dígitos.") { }
    }
    public class MesExpiracionDigitosException : Exception
    {
        public MesExpiracionDigitosException() : base("El mes de expiración debe estar entre 1 y 12.") { }
    }
    public class MarcaNullException : Exception
    {
        public MarcaNullException() : base("La marca de la tarjeta no puede estar vacía.") { }
    }
    public class MarcaIncompatibleException : Exception
    {
        public MarcaIncompatibleException(string marca) : base($"Marca de tarjeta no reconocida: '{marca}'.") { }
    }
    public class IdUsuarioNullException : Exception
    {
        public IdUsuarioNullException() : base("El ID de usuario no puede estar vacío.") { }
    }
    public class IdUsuarioInvalidoException : Exception
    {
        public IdUsuarioInvalidoException() : base("El ID de usuario debe ser un GUID válido.") { }
    }
    public class IdMPagoStripeNullException : Exception
    {
        public IdMPagoStripeNullException() : base("El ID del método de pago no puede estar vacío.") { }
    }
    public class IdMPagoStripeInvalidoException : Exception
    {
        public IdMPagoStripeInvalidoException() : base("El ID del método de pago debe comenzar con 'pm_'.") { }
    }
    public class IdMPagoNullException : Exception
    {
        public IdMPagoNullException() : base("El ID del metodo de pago no puede estar vacío.") { }
    }
    public class IdMPagoInvalidoException : Exception
    {
        public IdMPagoInvalidoException() : base("El ID del metodo de pago debe ser un GUID válido.") { }
    }
    public class IdClienteStripeNullException : Exception
    {
        public IdClienteStripeNullException() : base("El ID del cliente no puede estar vacío.") { }
    }
    public class IdClienteStripeInvalidoException : Exception
    {
        public IdClienteStripeInvalidoException() : base("El ID del cliente debe comenzar con 'cus_'.") { }
    }
    public class FechaRegistroInvalidaException : Exception
    {
        public FechaRegistroInvalidaException() : base("La fecha de registro no puede ser en el futuro.") { }
    }
    public class AnioExpiracionInvalidoException : Exception
    {
        public AnioExpiracionInvalidoException() : base("El año de expiración debe ser mayor al actual.") { }
    }

    //MONGODB EXCEPTIONS
    public class MongoDBConnectionException : Exception
    {
        public MongoDBConnectionException(Exception inner) : base("Error al conectar con la base de datos de mongo", inner) { }
    }
    public class MongoDBUnnexpectedException : Exception
    {
        public MongoDBUnnexpectedException(Exception inner) : base("Error inesperado con la base de datos de mongo", inner) { }
    }
    public class NombreBdInvalido : Exception
    {
        public NombreBdInvalido() : base("El nombre de la base de datos de MongoDB no está definido.") { }
    }
    public class ConexionBdMPagoInvalida : Exception
    {
        public ConexionBdMPagoInvalida() : base("La cadena de conexión de MongoDB no está definida.") { }
    }
    public class MongoDBCommandException : Exception
    {
        public MongoDBCommandException(Exception inner) : base("Error al ejecutar el comando en la base de datos.", inner) { }
    }
    public class MPagoWriteRepositoryException : Exception
    {
        public MPagoWriteRepositoryException(Exception inner) : base("Fallo en MPagoWriteRepository. " +
            "No se pudo completar la operación de escritura.", inner)
        { }
    }
    public class MPagoReadRepositoryException : Exception
    {
        public MPagoReadRepositoryException(Exception inner) : base("Fallo en MPagoReadRepository. " +
            "No se pudo completar la operación.", inner)
        { }
    }
    public class MPagoNullRepositoryException : Exception
    {
        public MPagoNullRepositoryException() : base("No se encontró el MPago que se esta buscando.")
        { }
    }
    public class MPagoOperacionRepositoryException : Exception
    {
        public MPagoOperacionRepositoryException() : base("No se pudo realizar la operacion en el MPago.")
        { }
    }

    //MEDIATR NULL EXCEPTION
    public class MediatorNullException : Exception
    {
        public MediatorNullException() : base("El componente IMediator no fue inicializado correctamente. " +
            "Asegúrate de que esté registrado en el contenedor de dependencias.")
        { }
    }

    //SENDENDPOINTPROVIDER NULL EXCEPTION
    public class SendEndpointProviderNullException : Exception
    {
        public SendEndpointProviderNullException() : base("No se pudo acceder al publicador de eventos (ISendEndpointProvider). " +
            "El servicio de mensajería no está disponible o no fue configurado.")
        { }
    }

    //PUBLISHENDPOINT NULL EXCEPTION
    public class PublishEndpointNullException : Exception
    {
        public PublishEndpointNullException() : base("No se pudo acceder al publicador de eventos (IPublishEndpoint). " +
            "El servicio de mensajería no está disponible o no fue configurado.")
        { }
    }

    //REPOSITORY NULL EXCEPTION
    public class MPagoWriteRepositoryNullException : Exception
    {
        public MPagoWriteRepositoryNullException() : base("El componente IMPagoRepository no fue inicializado correctamente. " +
            "Asegúrate de que esté registrado en el contenedor de dependencias.")
        { }
    }
    public class MPagoReadRepositoryNullException : Exception
    {
        public MPagoReadRepositoryNullException() : base("El componente IMPagoReadRepository no fue inicializado correctamente. " +
            "Asegúrate de que esté registrado en el contenedor de dependencias.")
        { }
    }

    //STRIPESERVICE EXCEPTIONS
    public class StripeServiceException : Exception
    {
        public StripeServiceException(Exception inner) : base("Error al conectarse con Stripe.", inner) { }
    }
    public class StripeServiceNullException : Exception
    {
        public StripeServiceNullException() : base("El componente IStripeService no fue inicializado correctamente. " +
            "Asegúrate de que esté registrado en el contenedor de dependencias.")
        { }
    }

    //FUNCIONALIDADES EXCEPTIONS (Commands, Events, Consumers)
    public class MPagoNullException : Exception
    {
        public MPagoNullException() : base("No se encontró el MPago que se esta buscando.")
        { }
    }
    public class MPagoIdUsuarioNullException : Exception
    {
        public MPagoIdUsuarioNullException() : base("No se encontraron MPagos para el postor especificado.")
        { }
    }
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
    public class AgregarMPagoConsumerException : Exception
    {
        public AgregarMPagoConsumerException(Exception inner)
            : base("Ocurrió un error al consumir el mensaje AgregarMPago desde la cola de eventos.", inner)
        { }
    }
    public class EliminarMPagoCommandHandlerException : Exception
    {
        public EliminarMPagoCommandHandlerException(Exception inner) : base("No fue posible eliminar el MPago al dominio. " +
            "El comando no cumplió con las reglas de negocio definidas.", inner)
        { }
    }
    public class MPagoEliminadoEventHandlerException : Exception
    {
        public MPagoEliminadoEventHandlerException(Exception inner)
            : base("El evento de MPago eliminado no pudo ser aplicado al estado del dominio.", inner)
        { }
    }
    public class EliminarMPagoConsumerException : Exception
    {
        public EliminarMPagoConsumerException(Exception inner)
            : base("Ocurrió un error al consumir el mensaje EliminarMPago desde la cola de eventos.", inner)
        { }
    }
    public class MPagoPredeterminadoCommandHandlerException : Exception
    {
        public MPagoPredeterminadoCommandHandlerException(Exception inner) : base("No fue posible colocar como predeterminado el MPago al dominio. " +
            "El comando no cumplió con las reglas de negocio definidas.", inner)
        { }
    }
    public class MPagoPredeterminadoEventHandlerException : Exception
    {
        public MPagoPredeterminadoEventHandlerException(Exception inner)
            : base("El evento de MPago predeterminado no pudo ser aplicado al estado del dominio.", inner)
        { }
    }
    public class MPagoPredeterminadoConsumerException : Exception
    {
        public MPagoPredeterminadoConsumerException(Exception inner)
            : base("Ocurrió un error al consumir el mensaje MPagoPredeterminado desde la cola de eventos.", inner)
        { }
    }

    //FUNCIONALIDADES EXCEPTIONS (Queries)
    public class GetTodosMPagoQueryHandlerException : Exception
    {
        public GetTodosMPagoQueryHandlerException(Exception inner)
            : base("El manejador de la consulta GetTodosMPago no pudo obtener la entidad MPago del repositorio.", inner)
        { }
    }
    public class GetMPagoPorIdQueryHandlerException : Exception
    {
        public GetMPagoPorIdQueryHandlerException(Exception inner)
            : base("El manejador de la consulta GetMPagoPorId no pudo obtener la entidad MPago del repositorio.", inner)
        { }
    }
    public class GetMPagoPorIdPostorQueryHandlerException : Exception
    {
        public GetMPagoPorIdPostorQueryHandlerException(Exception inner)
            : base("El manejador de la consulta GetMPagoPorIdPostor no pudo obtener la entidad MPago del repositorio.", inner)
        { }
    }
}
