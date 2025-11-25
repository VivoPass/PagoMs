using log4net;
using Pagos.Application.Interfaces;
using Pagos.Domain.Exceptions;
using Stripe;

namespace Pagos.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de integración para la plataforma de pagos Stripe.
    /// Gestiona la creación de tokens, clientes y el procesamiento de pagos.
    /// </summary>
    public class StripeService : IStripeService
    {
        private readonly ILog Log;
        public StripeService(ILog log)
        {
            StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
            Log = log ?? throw new LogNullException();
        }

        #region CrearTokenPM(string numeroTarjeta, int expMonth, int expYear, string cvc)
        /// <summary>
        /// Crea un token de pago temporal a partir de los detalles de una tarjeta.
        /// Este token se usa para crear un PaymentMethod o un Customer por primera vez.
        /// </summary>
        /// <param name="numeroTarjeta">Número completo de la tarjeta (solo se envía a Stripe, no se almacena).</param>
        /// <param name="expMonth">Mes de expiración.</param>
        /// <param name="expYear">Año de expiración.</param>
        /// <param name="cvc">Código de seguridad CVC.</param>
        /// <returns>Objeto Token de Stripe.</returns>
        /// <exception cref="StripeServiceException">Se lanza si ocurre un error durante la comunicación con Stripe.</exception>
        public async Task<Token> CrearTokenPM(string numeroTarjeta, int expMonth, int expYear, string cvc)
        {
            Log.Debug($"Iniciando creación de Token PM para tarjeta que expira: {expMonth}/{expYear}.");
            try
            {
                var tokenOptions = new TokenCreateOptions
                {
                    Card = new TokenCardOptions
                    { Number = numeroTarjeta, ExpMonth = expMonth.ToString(), ExpYear = expYear.ToString(), Cvc = cvc }
                };

                var service = new TokenService();
                Log.Info("Llamando a Stripe para crear el Token (service.CreateAsync).");
                var token = await service.CreateAsync(tokenOptions);

                Log.Info($"Token PM creado exitosamente. ID de Token: {token.Id}, Marca: {token.Card.Brand}, Últimos 4 dígitos: {token.Card.Last4}.");
                return token;
            }
            catch (Exception ex)
            {
                Log.Error($"Error al intentar crear el Token PM en Stripe. Tarjeta expira: {expMonth}/{expYear}.", ex);
                throw new StripeServiceException(ex);
            }
        }
        #endregion

        #region CrearTokenCUS(string email, string paymentMethodId)
        /// <summary>
        /// Crea un nuevo cliente en Stripe o recupera uno existente si el PaymentMethod ya está asociado a un Customer.
        /// Si el PaymentMethod no está asociado, crea un nuevo cliente y lo enlaza.
        /// </summary>
        /// <param name="email">Email del cliente.</param>
        /// <param name="paymentMethodId">ID del método de pago (PaymentMethod) a asociar.</param>
        /// <returns>Objeto Customer de Stripe.</returns>
        /// <exception cref="StripeServiceException">Se lanza si ocurre un error durante la comunicación con Stripe.</exception>
        /// <exception cref="ArgumentException">Se lanza si el método de pago proporcionado no existe.</exception>
        public async Task<Customer> CrearTokenCUS(string email, string paymentMethodId)
        {
            Log.Debug($"Iniciando proceso de creación/recuperación de Customer. Email: {email}, PaymentMethodId: {paymentMethodId}.");
            try
            {
                // buscar si este metodo de pago ya tiene asociado un cliente
                var paymentMethodService = new PaymentMethodService();

                Log.Debug($"Buscando PaymentMethod existente: {paymentMethodId}.");
                var existingPaymentMethod = await paymentMethodService.GetAsync(paymentMethodId);
                if (existingPaymentMethod == null)
                {
                    Log.Warn($"PaymentMethodId '{paymentMethodId}' no encontrado en Stripe. Abortando.");
                    throw new ArgumentException("El método de pago no existe.");
                }
                var customerService = new CustomerService();
                if (existingPaymentMethod.CustomerId != null)
                {
                    Log.Info($"PaymentMethod {paymentMethodId} ya está asociado al Customer ID: {existingPaymentMethod.CustomerId}. " +
                             $"Recuperando cliente existente.");
                    return await customerService.GetAsync(existingPaymentMethod.CustomerId);
                }

                Log.Info($"Creando nuevo Customer para el email: {email} y PaymentMethod: {paymentMethodId}.");
                var customerOptions = new CustomerCreateOptions
                {
                    Email = email,
                    PaymentMethod = paymentMethodId,
                    InvoiceSettings = new CustomerInvoiceSettingsOptions
                    {
                        DefaultPaymentMethod = paymentMethodId
                    }
                };

                var newCustomer = await customerService.CreateAsync(customerOptions);
                Log.Info($"Nuevo Customer creado exitosamente. ID: {newCustomer.Id}.");

                return newCustomer;
            }
            catch (ArgumentException)
            {
                throw;
            }

            catch (Exception ex)
            {
                Log.Error($"Error al crear o recuperar el Customer en Stripe. Email: {email}, PaymentMethodId: {paymentMethodId}.", ex);
                throw new StripeServiceException(ex);
            }
        }
        #endregion

        #region EliminarMPago(string customerId, string paymentMethodId)
        /// <summary>
        /// Desvincula un método de pago de un cliente en Stripe. Esto es equivalente a "eliminar" el método de pago guardado.
        /// </summary>
        /// <param name="customerId">ID del cliente (Customer) de Stripe.</param>
        /// <param name="paymentMethodId">ID del método de pago a desvincular.</param>
        /// <returns>Tarea asíncrona completada.</returns>
        /// <exception cref="StripeServiceException">Se lanza si ocurre un error durante la desvinculación.</exception>
        public async Task EliminarMPago(string customerId, string paymentMethodId)
        {
            Log.Debug($"Iniciando desvinculación (detach) del PaymentMethod {paymentMethodId} para el Customer {customerId}.");
            
            try
            {
                Log.Info("Llamando a Stripe para desvincular el método de pago (DetachAsync).");
                var paymentMethodService = new Stripe.PaymentMethodService();
                await paymentMethodService.DetachAsync(paymentMethodId);

                Log.Info($"PaymentMethod {paymentMethodId} desvinculado exitosamente del Customer {customerId}.");
            }
            catch (Exception ex)
            {
                Log.Error($"Error al intentar eliminar (Detach) el PaymentMethod {paymentMethodId} para el Customer {customerId}.", ex);
                throw new StripeServiceException(ex);
            }
        }
        #endregion

        #region RealizarPago(decimal monto, string customerId, string paymentMethodId)
        /// <summary>
        /// Realiza una transacción de pago utilizando un PaymentIntent off-session (con cargo a un método de pago guardado).
        /// </summary>
        /// <param name="monto">Monto del pago en la moneda base (USD asumida).</param>
        /// <param name="customerId">ID del cliente (Customer) de Stripe.</param>
        /// <param name="paymentMethodId">ID del método de pago guardado a utilizar.</param>
        /// <returns>El ID del PaymentIntent si el pago es exitoso, o null si falla la confirmación (lanzando excepción custom).</returns>
        /// <exception cref="StripeServiceException">Se lanza si ocurre un error general o de Stripe.</exception>
        /// <exception cref="PagoUnsuccessfulStripeServiceException">Se lanza si el PaymentIntent no tiene estado 'succeeded'.</exception>
        async public Task<string?> RealizarPago(decimal monto, string customerId, string paymentMethodId)
        {
            long montoEnCents = (long)(monto * 100);
            Log.Debug($"Iniciando pago off-session. Monto: {monto:F2} ({montoEnCents} cents), Customer: {customerId}, PaymentMethod: {paymentMethodId}.");

            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = montoEnCents,
                    Currency = "usd",
                    Customer = customerId,
                    PaymentMethod = paymentMethodId,
                    Confirm = true,
                    OffSession = true
                };

                var service = new PaymentIntentService();

                Log.Info($"Llamando a Stripe para crear y confirmar el PaymentIntent para {montoEnCents} cents.");
                var intent = await service.CreateAsync(options);

                Log.Debug($"PaymentIntent creado/procesado. ID: {intent.Id}, Estado: {intent.Status}.");
                if (intent.Status != "succeeded")
                {
                    Log.Error($"El pago con PaymentIntent {intent.Id} falló. Estado final: {intent.Status}.");
                    throw new PagoUnsuccessfulStripeServiceException();
                }

                Log.Info($"Pago exitoso. PaymentIntent ID: {intent.Id}.");
                return intent.Id;
            }
            catch (StripeException ex)
            {
                Log.Error($"Error específico de Stripe (StripeException) durante RealizarPago. StripeResponse: {ex.StripeResponse}, " +
                          $"Mensaje: {ex.Message}.", ex);
                throw new StripeServiceException(ex);
            }
            catch (Exception ex)
            {
                Log.Error("Error inesperado durante RealizarPago.", ex);
                throw new StripeServiceException(ex);
            }
        }
        #endregion
    }
}
