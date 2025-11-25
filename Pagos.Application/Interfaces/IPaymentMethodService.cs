using Stripe;

namespace Pagos.Application.Interfaces
{
    /// <summary>
    /// Contrato para interactuar con los servicios de métodos de pago de la pasarela (ej: Stripe).
    /// </summary>
    public interface IPaymentMethodService
    {
        /// <summary>
        /// Recupera los detalles completos de un método de pago.
        /// </summary>
        /// <param name="paymentMethodId">El ID del método de pago proporcionado por la pasarela (ej: pm_...)</param>
        /// <returns>Objeto PaymentMethod con los detalles.</returns>
        Task<PaymentMethod> GetAsync(string paymentMethodId);
    }
}
