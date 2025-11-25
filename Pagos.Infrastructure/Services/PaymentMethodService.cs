using Pagos.Application.Interfaces;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Infrastructure.Services
{
    /// <summary>
    /// Implementación concreta de la IPaymentMethodService que utiliza el SDK de Stripe.net.
    /// </summary>
    public class PaymentMethodService : IPaymentMethodService
    {
        private readonly Stripe.PaymentMethodService _stripePaymentMethodService;

        // El constructor del servicio de infraestructura puede recibir el servicio de Stripe 
        // o inicializarlo aquí, dependiendo de tu configuración de infraestructura.
        public PaymentMethodService()
        {
            // Inicialización de la clase real del SDK de Stripe (se asume que la API Key está configurada)
            _stripePaymentMethodService = new Stripe.PaymentMethodService();
        }

        public async Task<PaymentMethod> GetAsync(string paymentMethodId)
        {
            try
            {
                // Opción para incluir RequestOptions si es necesario (ej: pasando el API Key si no está global)
                return await _stripePaymentMethodService.GetAsync(paymentMethodId);
            }
            catch (StripeException ex)
            {
                // Re-lanzar la excepción para que sea capturada por el Command Handler
                throw ex;
            }
        }
    }
}
