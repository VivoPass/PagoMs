using log4net;
using Moq;
using Pagos.Application.Commands;
using Pagos.Application.Commands.CommandHandlers;
using Pagos.Domain.Entities;
using Pagos.Domain.Exceptions;
using Pagos.Domain.Interfaces;
using Pagos.Domain.ValueObjects;
using Xunit;

namespace Pagos.Tests.Pagos.Application.CommandHandlers
{
    public class CommandHandler_MPagoPredeterminado_Tests
    {
        private readonly Mock<IMPagoRepository> _mockRepo;
        private readonly Mock<ILog> _mockLog;
        private readonly MPagoPredeterminadoCommandHandler _handler;

        private readonly string _idUsuario;
        private readonly string _idMpagoNuevo;
        private readonly string _idMpagoAnterior;
        private readonly TarjetaCredito _mPagoNuevo;
        private readonly TarjetaCredito _mPagoAnteriorPredeterminado;

        private readonly MPagoPredeterminadoCommand _validCommand;

        public CommandHandler_MPagoPredeterminado_Tests()
        {
            _mockRepo = new Mock<IMPagoRepository>();
            _mockLog = new Mock<ILog>();

            _handler = new MPagoPredeterminadoCommandHandler(
                _mockRepo.Object,
                _mockLog.Object
            );

            _idUsuario = Guid.NewGuid().ToString();
            _idMpagoNuevo = Guid.NewGuid().ToString();
            _idMpagoAnterior = Guid.NewGuid().ToString();

            // MPago que queremos dejar como predeterminado
            _mPagoNuevo = new TarjetaCredito(
                idMPago: new VOIdMPago(_idMpagoNuevo),
                idUsuario: new VOIdUsuario(_idUsuario),
                idMPagoStripe: new VOIdMPagoStripe("pm_nuevo_123"),
                idClienteStripe: new VOIdClienteStripe("cus_123"),
                marca: new VOMarca("visa"),
                mesExpiracion: new VOMesExpiracion(12),
                anioExpiracion: new VOAnioExpiracion(2030),
                ultimos4: new VOUltimos4("4242"),
                fechaRegistro: new VOFechaRegistro(DateTime.UtcNow),
                predeterminado: new VOPredeterminado(false)
            );

            // MPago que actualmente es predeterminado
            _mPagoAnteriorPredeterminado = new TarjetaCredito(
                idMPago: new VOIdMPago(_idMpagoAnterior),
                idUsuario: new VOIdUsuario(_idUsuario),
                idMPagoStripe: new VOIdMPagoStripe("pm_old_456"),
                idClienteStripe: new VOIdClienteStripe("cus_123"),
                marca: new VOMarca("mastercard"),
                mesExpiracion: new VOMesExpiracion(11),
                anioExpiracion: new VOAnioExpiracion(2029),
                ultimos4: new VOUltimos4("5555"),
                fechaRegistro: new VOFechaRegistro(DateTime.UtcNow.AddMonths(-3)),
                predeterminado: new VOPredeterminado(true)
            );

            _validCommand = new MPagoPredeterminadoCommand(
                _idMpagoNuevo,
                _idUsuario
            );
        }

        #region Caso éxito: había otro predeterminado → se desactiva y se activa el nuevo
        [Fact]
        public async Task Handle_Success_WithPreviousPredeterminado_ReturnsTrue_AndUpdatesBoth()
        {
            // ARRANGE
            _mockRepo
                .Setup(r => r.ObtenerMPagoPorId(_idMpagoNuevo))
                .ReturnsAsync(_mPagoNuevo);

            _mockRepo
                .Setup(r => r.ObtenerMPagoPorIdUsuario(_idUsuario))
                .ReturnsAsync(new List<TarjetaCredito>
                {
                    _mPagoAnteriorPredeterminado,
                    _mPagoNuevo
                });

            _mockRepo
                .Setup(r => r.ActualizarPredeterminadoFalseMPago(_idMpagoAnterior))
                .Returns(Task.CompletedTask);

            _mockRepo
                .Setup(r => r.ActualizarPredeterminadoTrueMPago(_idMpagoNuevo))
                .Returns(Task.CompletedTask);

            // ACT
            var result = await _handler.Handle(_validCommand, CancellationToken.None);

            // ASSERT
            Assert.True(result);

            _mockRepo.Verify(r => r.ObtenerMPagoPorId(_idMpagoNuevo), Times.Once);
            _mockRepo.Verify(r => r.ObtenerMPagoPorIdUsuario(_idUsuario), Times.Once);
            _mockRepo.Verify(r => r.ActualizarPredeterminadoFalseMPago(_idMpagoAnterior), Times.Once);
            _mockRepo.Verify(r => r.ActualizarPredeterminadoTrueMPago(_idMpagoNuevo), Times.Once);
        }
        #endregion

        #region Caso éxito: NO había otro predeterminado → solo se marca el nuevo
        [Fact]
        public async Task Handle_Success_WithoutPreviousPredeterminado_OnlySetsNew()
        {
            // ARRANGE
            _mockRepo
                .Setup(r => r.ObtenerMPagoPorId(_idMpagoNuevo))
                .ReturnsAsync(_mPagoNuevo);

            // Ninguno viene marcado como predeterminado (o solo el mismo idMpago)
            var otroNoPred = new TarjetaCredito(
                idMPago: new VOIdMPago(Guid.NewGuid().ToString()),
                idUsuario: new VOIdUsuario(_idUsuario),
                idMPagoStripe: new VOIdMPagoStripe("pm_xxx"),
                idClienteStripe: new VOIdClienteStripe("cus_123"),
                marca: new VOMarca("visa"),
                mesExpiracion: new VOMesExpiracion(10),
                anioExpiracion: new VOAnioExpiracion(2028),
                ultimos4: new VOUltimos4("1111"),
                fechaRegistro: new VOFechaRegistro(DateTime.UtcNow.AddMonths(-1)),
                predeterminado: new VOPredeterminado(false)
            );

            _mockRepo
                .Setup(r => r.ObtenerMPagoPorIdUsuario(_idUsuario))
                .ReturnsAsync(new List<TarjetaCredito>
                {
                    otroNoPred,
                    _mPagoNuevo // este viene con Predeterminado = false
                });

            _mockRepo
                .Setup(r => r.ActualizarPredeterminadoTrueMPago(_idMpagoNuevo))
                .Returns(Task.CompletedTask);

            // ACT
            var result = await _handler.Handle(_validCommand, CancellationToken.None);

            // ASSERT
            Assert.True(result);

            _mockRepo.Verify(r => r.ActualizarPredeterminadoFalseMPago(It.IsAny<string>()), Times.Never);
            _mockRepo.Verify(r => r.ActualizarPredeterminadoTrueMPago(_idMpagoNuevo), Times.Once);
        }
        #endregion

        #region MPago no encontrado → MPagoNullException
        [Fact]
        public async Task Handle_MPagoNotFound_ThrowsMPagoNullException()
        {
            // ARRANGE
            _mockRepo
                .Setup(r => r.ObtenerMPagoPorId(_idMpagoNuevo))
                .ReturnsAsync((TarjetaCredito)null!);

            // ACT & ASSERT
            await Assert.ThrowsAsync<MPagoNullException>(
                async () => await _handler.Handle(_validCommand, CancellationToken.None)
            );

            _mockRepo.Verify(r => r.ObtenerMPagoPorIdUsuario(It.IsAny<string>()), Times.Never);
            _mockRepo.Verify(r => r.ActualizarPredeterminadoTrueMPago(It.IsAny<string>()), Times.Never);
        }
        #endregion

        #region Usuario sin MPagos → MPagoIdUsuarioNullException
        [Fact]
        public async Task Handle_UserWithoutMPagos_ThrowsMPagoIdUsuarioNullException()
        {
            // ARRANGE
            _mockRepo
                .Setup(r => r.ObtenerMPagoPorId(_idMpagoNuevo))
                .ReturnsAsync(_mPagoNuevo);

            // Sin MPagos
            _mockRepo
                .Setup(r => r.ObtenerMPagoPorIdUsuario(_idUsuario))
                .ReturnsAsync((List<TarjetaCredito>)null!);

            // ACT & ASSERT
            await Assert.ThrowsAsync<MPagoIdUsuarioNullException>(
                async () => await _handler.Handle(_validCommand, CancellationToken.None)
            );

            _mockRepo.Verify(r => r.ActualizarPredeterminadoFalseMPago(It.IsAny<string>()), Times.Never);
            _mockRepo.Verify(r => r.ActualizarPredeterminadoTrueMPago(It.IsAny<string>()), Times.Never);
        }
        #endregion

        #region Excepción inesperada → MPagoPredeterminadoCommandHandlerException
        [Fact]
        public async Task Handle_UnexpectedException_IsWrappedInMPagoPredeterminadoCommandHandlerException()
        {
            // ARRANGE: petamos el repo en la primera llamada
            _mockRepo
                .Setup(r => r.ObtenerMPagoPorId(_idMpagoNuevo))
                .ThrowsAsync(new Exception("Error inesperado en la DB"));

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<MPagoPredeterminadoCommandHandlerException>(
                async () => await _handler.Handle(_validCommand, CancellationToken.None)
            );

            Assert.NotNull(ex.InnerException);
            Assert.Equal("Error inesperado en la DB", ex.InnerException!.Message);

            _mockRepo.Verify(r => r.ObtenerMPagoPorIdUsuario(It.IsAny<string>()), Times.Never);
            _mockRepo.Verify(r => r.ActualizarPredeterminadoTrueMPago(It.IsAny<string>()), Times.Never);
        }
        #endregion
    }
}
