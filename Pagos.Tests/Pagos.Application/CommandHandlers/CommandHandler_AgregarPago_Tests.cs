using log4net;
using Moq;
using Pagos.Application.Commands;
using Pagos.Application.Commands.CommandHandlers;
using Pagos.Application.DTOs;
using Pagos.Application.Interfaces;
using Pagos.Domain.Aggregates;
using Pagos.Domain.Entities;
using Pagos.Domain.Exceptions;
using Pagos.Domain.Interfaces;
using Pagos.Domain.ValueObjects;
using Xunit;

namespace Pagos.Tests.Pagos.Application.CommandHandlers
{
    public class CommandHandler_AgregarPago_Tests
    {
        private readonly Mock<IPagoRepository> MockRepo;
        private readonly Mock<IPagoFactory> MockFactory;
        private readonly Mock<IStripeService> MockStripeService;
        private readonly Mock<ILog> MockLog;
        private readonly AgregarPagoCommandHandler Handler;

        // --- DATOS ---
        private readonly AgregarPagoCommand ValidCommand;
        private readonly AgregarPagoDTO ValidPagoDto;
        private readonly Pago MockPagoEntity;
        private readonly string ExpectedPagoId;
        private readonly string StripeCustomerId = "cus_123456789";
        private readonly string StripePaymentMethodId = "pm_987654321";
        private readonly string StripeExternalPaymentId = "pay_ext_999";

        public CommandHandler_AgregarPago_Tests()
        {
            MockRepo = new Mock<IPagoRepository>();
            MockFactory = new Mock<IPagoFactory>();
            MockStripeService = new Mock<IStripeService>();
            MockLog = new Mock<ILog>();

            Handler = new AgregarPagoCommandHandler(
                MockRepo.Object,
                MockFactory.Object,
                MockStripeService.Object,
                MockLog.Object
            );

            ExpectedPagoId = Guid.NewGuid().ToString();

            // ⚠ Estos IDs deben ser GUID válidos para que los VO no exploten
            var idUsuarioGuid = Guid.NewGuid().ToString();
            var idMPagoGuid = Guid.NewGuid().ToString();
            var idReservaGuid = Guid.NewGuid().ToString();
            var idEventoGuid = Guid.NewGuid().ToString();

            ValidPagoDto = new AgregarPagoDTO
            {
                IdMPago = idMPagoGuid,
                IdUsuario = idUsuarioGuid,
                IdReserva = idReservaGuid,
                IdEvento = idEventoGuid,
                FechaPago = DateTime.UtcNow,
                Monto = 150.75m
            };

            // Este idUsuario / idMPago del command son los que usa el PaymentService (Stripe),
            // pueden ser los “cus_ / pm_” sin problema, porque ahí NO se crean VOs.
            ValidCommand = new AgregarPagoCommand(
                ValidPagoDto,
                StripeCustomerId,      // Pago.IdUsuario para Stripe
                StripePaymentMethodId  // Pago.IdMPago para Stripe
            );

            // Entidad de dominio Pago simulada
            MockPagoEntity = new Pago(
                new VOIdPago(ExpectedPagoId),          // 1
                new VOIdMPago(ValidPagoDto.IdMPago),   // 2
                new VOIdUsuario(ValidPagoDto.IdUsuario), // 3
                new VOFechaPago(ValidPagoDto.FechaPago), // 4
                new VOMonto(ValidPagoDto.Monto),       // 5
                new VOIdReserva(ValidPagoDto.IdReserva), // 6
                new VOIdEvento(ValidPagoDto.IdEvento), // 7
                null                                   // 8 → VOIdExternalPago? (lo dejas en null al inicio)
            );
        }

        #region Handle_SuccessCase_ReturnsExpectedId
        [Fact]
        public async Task Handle_SuccessCase_ReturnsExpectedId()
        {
            // ARRANGE
            MockFactory.Setup(f => f.Crear(
                    ValidPagoDto.IdMPago,
                    ValidPagoDto.IdUsuario,
                    ValidPagoDto.FechaPago,
                    ValidPagoDto.Monto,
                    ValidPagoDto.IdReserva,
                    ValidPagoDto.IdEvento,
                    null
                ))
                .Returns(MockPagoEntity);

            MockRepo
                .Setup(r => r.AgregarPago(It.IsAny<Pago>()))
                .Returns(Task.CompletedTask);

            MockStripeService
                .Setup(s => s.RealizarPago(
                    ValidPagoDto.Monto,
                    ValidCommand.IdUsuario,
                    ValidCommand.IdMPago))
                .ReturnsAsync(StripeExternalPaymentId);

            MockRepo
                .Setup(r => r.ActualizarIdPagoExterno(
                    ExpectedPagoId,
                    StripeExternalPaymentId))
                .Returns(Task.CompletedTask);

            // ACT
            var result = await Handler.Handle(ValidCommand, CancellationToken.None);

            // ASSERT
            Assert.Equal(ExpectedPagoId, result);
        }
        #endregion

        #region Handle_SuccessCase_VerifyCollaborations
        [Fact]
        public async Task Handle_SuccessCase_VerifyCollaborations()
        {
            // ARRANGE
            MockFactory.Setup(f => f.Crear(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<decimal>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>()
                ))
                .Returns(MockPagoEntity);

            MockRepo
                .Setup(r => r.AgregarPago(It.IsAny<Pago>()))
                .Returns(Task.CompletedTask);

            MockStripeService
                .Setup(s => s.RealizarPago(
                    It.IsAny<decimal>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(StripeExternalPaymentId);

            MockRepo
                .Setup(r => r.ActualizarIdPagoExterno(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // ACT
            await Handler.Handle(ValidCommand, CancellationToken.None);

            // ASSERT
            MockFactory.Verify(f => f.Crear(
                    ValidPagoDto.IdMPago,
                    ValidPagoDto.IdUsuario,
                    ValidPagoDto.FechaPago,
                    ValidPagoDto.Monto,
                    ValidPagoDto.IdReserva,
                    ValidPagoDto.IdEvento,
                    null
                ),
                Times.Once);

            MockRepo.Verify(r => r.AgregarPago(
                    It.Is<Pago>(p => p.IdPago.Valor == ExpectedPagoId)),
                Times.Once);

            MockStripeService.Verify(s => s.RealizarPago(
                    ValidPagoDto.Monto,
                    ValidCommand.IdUsuario,
                    ValidCommand.IdMPago),
                Times.Once);

            MockRepo.Verify(r => r.ActualizarIdPagoExterno(
                    ExpectedPagoId,
                    StripeExternalPaymentId),
                Times.Once);
        }
        #endregion

        #region Handle_PaymentFails_ThrowsPagoUnsuccessfulException
        [Fact]
        public async Task Handle_PaymentFails_ThrowsPagoUnsuccessfulException()
        {
            MockFactory.Setup(f => f.Crear(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<decimal>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>()
                ))
                .Returns(MockPagoEntity);

            MockRepo
                .Setup(r => r.AgregarPago(It.IsAny<Pago>()))
                .Returns(Task.CompletedTask);

            // Stripe devuelve null
            MockStripeService
                .Setup(s => s.RealizarPago(
                    It.IsAny<decimal>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync((string)null!);

            await Assert.ThrowsAsync<PagoUnsuccessfulException>(
                async () => await Handler.Handle(ValidCommand, CancellationToken.None)
            );

            MockRepo.Verify(r => r.ActualizarIdPagoExterno(
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }
        #endregion

        #region Handle_UnexpectedException_WrappedInAgregarPagoCommandHandlerException
        [Fact]
        public async Task Handle_UnexpectedException_IsWrappedInAgregarPagoCommandHandlerException()
        {
            MockFactory.Setup(f => f.Crear(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<decimal>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>()
                ))
                .Returns(MockPagoEntity);

            MockRepo
                .Setup(r => r.AgregarPago(It.IsAny<Pago>()))
                .ThrowsAsync(new Exception("Error inesperado en la DB"));

            var ex = await Assert.ThrowsAsync<AgregarPagoCommandHandlerException>(
                async () => await Handler.Handle(ValidCommand, CancellationToken.None)
            );

            Assert.NotNull(ex.InnerException);
            Assert.Equal("Error inesperado en la DB", ex.InnerException!.Message);
        }
        #endregion
    }
}
