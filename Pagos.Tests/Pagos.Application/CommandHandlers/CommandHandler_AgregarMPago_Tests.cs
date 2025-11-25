using log4net;
using Moq;
using Pagos.Application.Commands;
using Pagos.Application.Commands.CommandHandlers;
using Pagos.Application.DTOs;
using Pagos.Application.Interfaces;
using Pagos.Domain.Entities;
using Pagos.Domain.Interfaces;
using Pagos.Domain.ValueObjects;
using Stripe;

namespace Pagos.Tests.Pagos.Application.CommandHandlers
{
    public class CommandHandler_AgregarMPago_Tests
    {
        private readonly Mock<IMPagoRepository> MockRepo;
        private readonly Mock<IStripeService> MockStripeService;
        private readonly Mock<IPaymentMethodService> MockPaymentMethodService;
        private readonly Mock<ITarjetaCreditoFactory> MockFactory;
        private readonly Mock<ILog> MockLog;
        private readonly AgregarMPagoCommandHandler Handler;

        // --- DATOS ---
        private readonly AgregarMPagoCommand ValidCommand;
        private readonly TarjetaCredito MockMPagoEntity;
        private readonly PaymentMethod MockPaymentMethod;
        private readonly string expectedId;
        private readonly string MockStripeCustomerId = "cus_stripe_customer_999";

        public CommandHandler_AgregarMPago_Tests()
        {
            MockRepo = new Mock<IMPagoRepository>();
            MockStripeService = new Mock<IStripeService>();
            MockPaymentMethodService = new Mock<IPaymentMethodService>();
            MockFactory = new Mock<ITarjetaCreditoFactory>();
            MockLog = new Mock<ILog>();
            Handler = new AgregarMPagoCommandHandler(MockRepo.Object, MockStripeService.Object, MockFactory.Object, MockLog.Object, MockPaymentMethodService.Object);

            // --- DATOS ---
            expectedId = Guid.NewGuid().ToString();

            ValidCommand = new AgregarMPagoCommand(new AgregarMPagoStripeDTO
            {
                IdUsuario = Guid.NewGuid().ToString(),
                IdMPagoStripe = "pm_1O3lR7I5xL6hV2e9oPqYzA8S",
                CorreoUsuario = "test@gmail.com"
            });
            // Configuración del objeto PaymentMethod (para simular Stripe SDK)
            MockPaymentMethod = new Stripe.PaymentMethod
            { Card = new Stripe.PaymentMethodCard { Last4 = "4242", Brand = "visa", ExpMonth = 12, ExpYear = 2030 } };

            MockMPagoEntity = new TarjetaCredito
            (
                idMPago: new VOIdMPago(expectedId),
                idUsuario: new VOIdUsuario(ValidCommand.MPagoStripe.IdUsuario),
                idMPagoStripe: new VOIdMPagoStripe(ValidCommand.MPagoStripe.IdMPagoStripe),
                idClienteStripe: new VOIdClienteStripe(MockStripeCustomerId),
                marca: new VOMarca(MockPaymentMethod.Card.Brand),
                mesExpiracion: new VOMesExpiracion(MockPaymentMethod.Card.ExpMonth.GetHashCode()),
                anioExpiracion: new VOAnioExpiracion(MockPaymentMethod.Card.ExpYear.GetHashCode()),
                ultimos4: new VOUltimos4(MockPaymentMethod.Card.Last4),
                fechaRegistro: new VOFechaRegistro(DateTime.Now),
                predeterminado: new VOPredeterminado(true)
            );
        }

        #region Handle_SuccessCase_ReturnsExpectedId()
        [Fact]
        public async Task Handle_SuccessCase_ReturnsExpectedId()
        {
            // ARRANGE
            MockStripeService.Setup(s => s.CrearTokenCUS(
                It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new Customer { Id = MockStripeCustomerId });
            MockPaymentMethodService.Setup(s => s.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(MockPaymentMethod));

            MockFactory.Setup(f => f.Crear(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<bool>()
            )).Returns(MockMPagoEntity);

            MockRepo.Setup(r => r.AgregarMPago(It.IsAny<TarjetaCredito>())).ReturnsAsync(MockMPagoEntity);

            // ACT
            var result = await Handler.Handle(ValidCommand, CancellationToken.None);

            // ASSERT
            Assert.Equal(expectedId, result);
        }
        #endregion

        #region Handle_SuccessCase_VarifyStripeService()
        [Fact]
        public async Task Handle_SuccessCase_VarifyStripeService()
        {
            // ARRANGE
            MockStripeService.Setup(s => s.CrearTokenCUS(
                It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new Customer { Id = MockStripeCustomerId });
            MockPaymentMethodService.Setup(s => s.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(MockPaymentMethod));

            MockFactory.Setup(f => f.Crear(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<bool>()
            )).Returns(MockMPagoEntity);

            MockRepo.Setup(r => r.AgregarMPago(It.IsAny<TarjetaCredito>())).ReturnsAsync(MockMPagoEntity);

            // ACT
            var result = await Handler.Handle(ValidCommand, CancellationToken.None);

            // ASSERT
            MockStripeService.Verify(s => s.CrearTokenCUS(
                ValidCommand.MPagoStripe.CorreoUsuario, // Verifica que se pasa el correo
                ValidCommand.MPagoStripe.IdMPagoStripe // Verifica que se pasa el token
            ), Times.Once);
        }
        #endregion

    }
}
