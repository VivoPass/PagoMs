using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Domain.Exceptions
{
    public class IdPagoInvalido : Exception
    {
        public IdPagoInvalido() : base("El ID del pago no es válido.") { }
    }
    public class IdUsuarioInvalido : Exception
    {
        public IdUsuarioInvalido() : base("El ID del usuario no es válido.") { }
    }
    public class IdAuctionInvalido : Exception
    {
        public IdAuctionInvalido() : base("El ID de la subasta no es válido.") { }
    }
    public class IdMPagoInvalido : Exception
    {
        public IdMPagoInvalido() : base("El ID del método de pago no es válido.") { }
    }
    public class MontoInvalido : Exception
    {
        public MontoInvalido() : base("El monto del pago debe ser mayor a cero.") { }
    }

    //MONGODB EXCEPTIONS
    public class ConexionBdPagoInvalida : Exception
    {
        public ConexionBdPagoInvalida() : base("La cadena de conexión de MongoDB no está definida.") { }
    }
    /*
    public class NombreBdInvalido : Exception
    {
        public NombreBdInvalido() : base("El nombre de la base de datos de MongoDB no está definido.") { }
    }
    public class ErrorConexionBd : Exception
    {
        public ErrorConexionBd(Exception inner) : base("No se pudo conectar a la base de datos.", inner) { }
    }

    public class MongoDBUnnexpectedException : Exception
    {
        public MongoDBUnnexpectedException(Exception inner) : base("Error inesperado con la base de datos de mongo", inner) { }
    }
    */
    public class PagoWriteRepositoryException : Exception
    {
        public PagoWriteRepositoryException(Exception inner) : base("Fallo en PagoWriteRepository. " +
            "No se pudo completar la operación de escritura.", inner)
        { }
    }
    public class PagoReadRepositoryException : Exception
    {
        public PagoReadRepositoryException(Exception inner) : base("Fallo en PagoReadRepository. " +
            "No se pudo completar la operación.", inner)
        { }
    }
    public class PagoNullRepositoryException : Exception
    {
        public PagoNullRepositoryException() : base("No se encontró el Pago que se esta buscando.")
        { }
    }
    /*
    //STRIPESERVICE EXCEPTIONS
    public class StripeServiceException : Exception
    {
        public StripeServiceException(Exception inner) : base("Error al conectarse con Stripe.", inner) { }
    }
    public class PagoUnsuccessfulStripeServiceException : Exception
    {
        public PagoUnsuccessfulStripeServiceException() : base("Error al realizar el pago por Stripe. No fue exitoso.") { }
    }

    public class StripeServiceNullException : Exception
    {
        public StripeServiceNullException() : base("El componente IStripeService no fue inicializado correctamente. " +
            "Asegúrate de que esté registrado en el contenedor de dependencias.")
        { }
    }

    //MEDIATR NULL EXCEPTION
    public class MediatorNullException : Exception
    {
        public MediatorNullException() : base("El componente IMediator no fue inicializado correctamente. " +
            "Asegúrate de que esté registrado en el contenedor de dependencias.")
        { }
    }

    //RESTCLIENT NULL EXCEPTION
    public class RestClientNullException : Exception
    {
        public RestClientNullException() : base("El servicio de cliente HTTP (IRestClient) no está disponible. " +
            "Comprueba su registro en el contenedor.")
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
    */

    //PAYMENTSERVICE NULL EXCEPTION
    public class PaymentServiceNullException : Exception
    {
        public PaymentServiceNullException() : base("El componente IPaymentService no fue inicializado correctamente. " +
            "Asegúrate de que esté registrado en el contenedor de dependencias.")
        { }
    }

    //REPOSITORY NULL EXCEPTION
    public class PagoWriteRepositoryNullException : Exception
    {
        public PagoWriteRepositoryNullException() : base("El componente IPagoRepository no fue inicializado correctamente. " +
            "Asegúrate de que esté registrado en el contenedor de dependencias.")
        { }
    }
    public class PagoReadRepositoryNullException : Exception
    {
        public PagoReadRepositoryNullException() : base("El componente IPagoReadRepository no fue inicializado correctamente. " +
            "Asegúrate de que esté registrado en el contenedor de dependencias.")
        { }
    }

    //FUNCIONALIDADES EXCEPTIONS (Commands, Events, Consumers)
    public class PagoNullException : Exception
    {
        public PagoNullException() : base("No se encontró el Pago que se esta buscando.")
        { }
    }
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
    public class AgregarPagoEventHandlerException : Exception
    {
        public AgregarPagoEventHandlerException(Exception inner)
            : base("El evento de Pago agregado no pudo ser aplicado al estado del dominio.", inner)
        { }
    }
    public class AgregarPagoConsumerException : Exception
    {
        public AgregarPagoConsumerException(Exception inner)
            : base("Ocurrió un error al consumir el mensaje AgregarPago desde la cola de eventos.", inner)
        { }
    }

    //FUNCIONALIDADES EXCEPTIONS (Queries)
    public class GetPagosUsuarioQueryHandlerException : Exception
    {
        public GetPagosUsuarioQueryHandlerException(Exception inner)
            : base("El manejador de la consulta GetPagosUsuario no pudo obtener la entidad Pago del repositorio.", inner)
        { }
    }
    public class GetPagoPorIdSubastaQueryHandlerException : Exception
    {
        public GetPagoPorIdSubastaQueryHandlerException(Exception inner)
            : base("El manejador de la consulta GetPagoPorIdSubasta no pudo obtener la entidad Pago del repositorio.", inner)
        { }
    }
    public class GetPagoPorIdQueryHandlerException : Exception
    {
        public GetPagoPorIdQueryHandlerException(Exception inner)
            : base("El manejador de la consulta GetPagoPorId no pudo obtener la entidad Pago del repositorio.", inner)
        { }
    }

    //CONTROLLER
    public class InfoUsuarioException : Exception
    {
        public InfoUsuarioException() : base("Error al obtener la información del usuario.")
        { }
    }
    public class InfoCompradorException : Exception
    {
        public InfoCompradorException() : base("Error al obtener la información del comprador.") { }
    }
}
