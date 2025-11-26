using log4net;
using Moq;
using Pagos.Application.Commands;
using Pagos.Application.Commands.CommandHandlers;
using Pagos.Application.Interfaces;
using Pagos.Domain.Entities;
using Pagos.Domain.Exceptions;
using Pagos.Domain.Interfaces;
using Pagos.Domain.ValueObjects;
using Xunit;

namespace Pagos.Tests.Pagos.Application.CommandHandlers
{
    public class CommandHandler_EliminarMPago_Tests
    {
        private readonly Mock<IMPagoRepository> MockRepo;
        private readonly Mock<IStripeService> MockStripeService;
        private readonly Mock<ILog> MockLog;
        private readonly EliminarMPagoCommandHandler Handler;

        // Datos de prueba
        private readonly string IdMpagoLocal;
        private readonly string IdClienteStripe;
        private readonly string IdMpagoStripe;
        private readonly TarjetaCredito MockMPagoEntity;
        private readonly EliminarMPagoCommand ValidCommand;

        public CommandHandler_EliminarMPago_Tests()
        {
            MockRepo = new Mock<IMPagoRepository>();
            MockStripeService = new Mock<IStripeService>();
            MockLog = new Mock<ILog>();

            Handler = new EliminarMPagoCommandHandler(
                MockRepo.Object,
                MockStripeService.Object,
                MockLog.Object
            );

            IdMpagoLocal = Guid.NewGuid().ToString();
            IdClienteStripe = "cus_123456789";
            IdMpagoStripe = "pm_987654321";

            // Command válido
            ValidCommand = new EliminarMPagoCommand(IdMpagoLocal);

            // Entidad TarjetaCredito simulada (igual estilo que en tus otros tests)
            MockMPagoEntity = new TarjetaCredito(
                idMPago: new VOIdMPago(IdMpagoLocal),
                idUsuario: new VOIdUsuario(Guid.NewGuid().ToString()),
                idMPagoStripe: new VOIdMPagoStripe(IdMpagoStripe),
                idClienteStripe: new VOIdClienteStripe(IdClienteStripe),
                marca: new VOMarca("visa"),
                mesExpiracion: new VOMesExpiracion(12),
                anioExpiracion: new VOAnioExpiracion(2030),
                ultimos4: new VOUltimos4("4242"),
                fechaRegistro: new VOFechaRegistro(DateTime.UtcNow),
                predeterminado: new VOPredeterminado(false)
            );
        }

        #region Handle_SuccessCase_ReturnsTrue_AndCallsStripeAndRepo
        [Fact]
        public async Task Handle_SuccessCase_ReturnsTrue_AndCallsStripeAndRepo()
        {
            // ARRANGE
            MockRepo
                .Setup(r => r.ObtenerMPagoPorId(IdMpagoLocal))
                .ReturnsAsync(MockMPagoEntity);

            MockStripeService
                .Setup(s => s.EliminarMPago(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            MockRepo
                .Setup(r => r.EliminarMPago(IdMpagoLocal))
                .Returns(Task.CompletedTask);

            // ACT
            var result = await Handler.Handle(ValidCommand, CancellationToken.None);

            // ASSERT
            Assert.True(result);

            // Verificamos colaboraciones
            MockRepo.Verify(r => r.ObtenerMPagoPorId(IdMpagoLocal), Times.Once);

            MockStripeService.Verify(s => s.EliminarMPago(
                    IdClienteStripe,
                    IdMpagoStripe),
                Times.Once);

            MockRepo.Verify(r => r.EliminarMPago(IdMpagoLocal), Times.Once);
        }
        #endregion

        #region Handle_NotFound_ThrowsMPagoNullException
        [Fact]
        public async Task Handle_NotFound_ThrowsMPagoNullException()
        {
            // ARRANGE: repo no encuentra el MPago
            MockRepo
                .Setup(r => r.ObtenerMPagoPorId(IdMpagoLocal))
                .ReturnsAsync((TarjetaCredito)null!);

            // ACT & ASSERT
            await Assert.ThrowsAsync<MPagoNullException>(
                async () => await Handler.Handle(ValidCommand, CancellationToken.None)
            );

            // No se deben llamar Stripe ni EliminarMPago del repo
            MockStripeService.Verify(s => s.EliminarMPago(
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);

            MockRepo.Verify(r => r.EliminarMPago(It.IsAny<string>()), Times.Never);
        }
        #endregion

        #region Handle_UnexpectedException_WrappedInEliminarMPagoCommandHandlerException
        [Fact]
        public async Task Handle_UnexpectedException_IsWrappedInEliminarMPagoCommandHandlerException()
        {
            // ARRANGE: repo lanza excepción inesperada al buscar
            MockRepo
                .Setup(r => r.ObtenerMPagoPorId(IdMpagoLocal))
                .ThrowsAsync(new Exception("Error inesperado en la DB"));

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<EliminarMPagoCommandHandlerException>(
                async () => await Handler.Handle(ValidCommand, CancellationToken.None)
            );

            Assert.NotNull(ex.InnerException);
            Assert.Equal("Error inesperado en la DB", ex.InnerException!.Message);

            // En este caso tampoco debe llegar a Stripe ni a EliminarMPago local
            MockStripeService.Verify(s => s.EliminarMPago(
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);

            MockRepo.Verify(r => r.EliminarMPago(It.IsAny<string>()), Times.Never);
        }
        #endregion
    }
}
