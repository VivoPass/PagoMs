using Stripe;
using System.Text.Json;
using Pagos.Domain.Exceptions;
using Pagos.Application.Interfaces;

namespace Pagos.Infrastructure.Services
{
    /// <summary>
    /// Servicio para interactuar con la API de Stripe: crear tokens de método de pago,
    /// clientes y eliminar métodos de pago.
    /// </summary>
    public class StripeService : IStripeService
    {
        /// <summary>
        /// Inicializa la clave secreta de Stripe desde variables de entorno.
        /// </summary>
        public StripeService()
        {
            StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
        }

        #region CrearTokenPM(string numeroTarjeta, int expMonth, int expYear, string cvc)
        /// <summary>
        /// Crea un token de tarjeta de pago (PaymentMethod) en Stripe.
        /// </summary>
        /// <param name="numeroTarjeta">Número de la tarjeta de crédito.</param>
        /// <param name="expMonth">Mes de expiración (1-12).</param>
        /// <param name="expYear">Año de expiración (4 dígitos).</param>
        /// <param name="cvc">Código de seguridad de la tarjeta.</param>
        /// <returns>
        /// <see cref="Token"/> generado por Stripe que representa el método de pago.
        /// </returns>
        /// <exception cref="StripeServiceException">
        /// Se lanza si ocurre un error al comunicarse con Stripe.
        /// </exception>
        public async Task<Token> CrearTokenPM(string numeroTarjeta, int expMonth, int expYear, string cvc)
        {
            try
            {
                var tokenOptions = new TokenCreateOptions
                {
                    Card = new TokenCardOptions
                    {
                        Number = numeroTarjeta,
                        ExpMonth = expMonth.ToString(),
                        ExpYear = expYear.ToString(),
                        Cvc = cvc
                    }
                };

                var service = new TokenService();
                return await service.CreateAsync(tokenOptions);
            }
            catch (Exception ex)
            {

                throw new StripeServiceException(ex);
            }
        }
        #endregion

        #region CrearTokenCUS(string email, string paymentMethodId)
        /// <summary>
        /// Crea o recupera un cliente en Stripe asociado a un método de pago existente.
        /// </summary>
        /// <param name="email">Correo electrónico del cliente.</param>
        /// <param name="paymentMethodId">ID del método de pago en Stripe.</param>
        /// <returns>
        /// <see cref="Customer"/> creado o recuperado de Stripe.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Se lanza si el método de pago especificado no existe.
        /// </exception>
        /// <exception cref="StripeServiceException">
        /// Se lanza si ocurre un error al comunicarse con Stripe.
        /// </exception>
        public async Task<Customer> CrearTokenCUS(string email, string paymentMethodId)
        {
            try
            {
                // buscar si este metodo de pago ya tiene asociado un cliente
                var paymentMethodService = new PaymentMethodService();
                var existingPaymentMethod = await paymentMethodService.GetAsync(paymentMethodId);
                if (existingPaymentMethod == null)
                {
                    throw new ArgumentException("El método de pago no existe.");
                }
                var customerService = new CustomerService();
                if (existingPaymentMethod.CustomerId != null)
                {
                    return await customerService.GetAsync(existingPaymentMethod.CustomerId);
                }
                var customerOptions = new CustomerCreateOptions
                {
                    Email = email,
                    PaymentMethod = paymentMethodId,
                    InvoiceSettings = new CustomerInvoiceSettingsOptions
                    {
                        DefaultPaymentMethod = paymentMethodId
                    }
                };

                return await customerService.CreateAsync(customerOptions);
            }
            catch (Exception ex)
            {
                throw new StripeServiceException(ex);
            }
        }
        #endregion

        #region EliminarMPago(string customerId, string paymentMethodId)
        /// <summary>
        /// Desvincula un método de pago de un cliente en Stripe.
        /// </summary>
        /// <param name="customerId">ID del cliente en Stripe.</param>
        /// <param name="paymentMethodId">ID del método de pago a eliminar.</param>
        /// <returns>Tarea que representa la operación asíncrona.</returns>
        /// <exception cref="StripeServiceException">
        /// Se lanza si ocurre un error al comunicarse con Stripe.
        /// </exception>
        public async Task EliminarMPago(string customerId, string paymentMethodId)
        {
            try
            {
                var paymentMethodService = new PaymentMethodService();

                await paymentMethodService.DetachAsync(paymentMethodId);
            }
            catch (Exception ex)
            {
                throw new StripeServiceException(ex);
            }
        }
        #endregion

    }
}
