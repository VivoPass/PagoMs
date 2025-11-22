using Stripe;

namespace Pagos.Application.Interfaces
{
    public interface IStripeService
    {
        Task<Token> CrearTokenPM(string numeroTarjeta, int expMonth, int expYear, string cvc);
        Task<Customer> CrearTokenCUS(string email, string paymentMethodId);
        Task EliminarMPago(string customerId, string paymentMethodId);
    }
}
