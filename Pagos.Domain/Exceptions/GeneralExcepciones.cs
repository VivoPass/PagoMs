
namespace Pagos.Domain.Exceptions
{
    #region CONFIGURATIONS EXCEPTIONS
    public class ConexionBdInvalida : Exception
    {
        public ConexionBdInvalida() : base("La cadena de conexión de MongoDB no está definida.") { }
    }
    public class NombreBdInvalido : Exception
    {
        public NombreBdInvalido() : base("El nombre de la base de datos de MongoDB no está definido.") { }
    }
    public class MongoDBConnectionException : Exception
    {
        public MongoDBConnectionException(Exception inner) : base("Error al conectar con la base de datos de mongo", inner) { }
    }
    public class MongoDBUnnexpectedException : Exception
    {
        public MongoDBUnnexpectedException(Exception inner) : base("Error inesperado con la base de datos de mongo", inner) { }
    }
    #endregion

    #region NULL EXCEPTIONS
    //MEDIATR NULL EXCEPTION
    public class MediatorNullException : Exception
    {
        public MediatorNullException() : base("El componente IMediator no fue inicializado correctamente. " +
                                              "Asegúrate de que esté registrado en el contenedor de dependencias.")
        { }
    }

    //LOG NULL EXCEPTION
    public class LogNullException : Exception
    {
        public LogNullException() : base("El servicio de logging (ILog o similar) es obligatorio y no puede ser nulo. " +
                                         "Asegúrese de que ILog esté correctamente inyectado en el constructor del componente.")
        { }
    }

    //RESTCLIENT NULL EXCEPTION
    public class RestClientNullException : Exception
    {
        public RestClientNullException() : base("El servicio de cliente HTTP (IRestClient) no está disponible. " +
                                                "Comprueba su registro en el contenedor.")
        { }
    }
    #endregion

    #region STRIPESERVICE EXCEPTIONS
    public class StripeServiceNullException : Exception
    {
        public StripeServiceNullException() : base("El componente IStripeService no fue inicializado correctamente. " +
                                                   "Asegúrate de que esté registrado en el contenedor de dependencias.")
        { }
    }
    public class StripeServiceException : Exception
    {
        public StripeServiceException(Exception inner) : base("Error al conectarse con Stripe.", inner) { }
    }
    public class PagoUnsuccessfulStripeServiceException : Exception
    {
        public PagoUnsuccessfulStripeServiceException() : base("Error al realizar el pago por Stripe. No fue exitoso.") { }
    }
    #endregion

    #region REPOSITORIES EXCEPTIONS
    //AuditoriaRepository
    public class AuditoriaRepositoryNullException : Exception
    {
        public AuditoriaRepositoryNullException() : base("El componente IAuditoriaRepository no fue inicializado correctamente. " +
                                                         "Asegúrate de que esté registrado en el contenedor de dependencias.")
        { }
    }
    public class AuditoriaRepositoryException : Exception
    {
        public AuditoriaRepositoryException(Exception inner) : base("Fallo en AuditoriaRepository. No se pudo completar la operación.", inner)
        { }
    }
    #endregion
}
