using log4net;
using Moq;
using Pagos.Application.DTOs;
using Pagos.Domain.Entities;
using Pagos.Domain.Exceptions;
using Pagos.Domain.Interfaces;
using Pagos.Domain.ValueObjects;
using Pagos.Infrastructure.Queries;
using Pagos.Infrastructure.Queries.QueryHandlers;
using Xunit;

namespace Pagos.Tests.Pagos.Infrastructure.QueryHandlers
{
    public class GetMPagoPorIdQueryHandler_Tests
    {
        private readonly Mock<IMPagoRepository> _mockRepo;
        private readonly Mock<ILog> _mockLog;
        private readonly GetMPagoPorIdQueryHandler _handler;

        private readonly string _idMpago;
        private readonly string _idUsuario;
        private readonly string _idMpagoStripe;
        private readonly string _idClienteStripe;
        private readonly TarjetaCredito _mPagoEntity;

        public GetMPagoPorIdQueryHandler_Tests()
        {
            _mockRepo = new Mock<IMPagoRepository>();
            _mockLog = new Mock<ILog>();

            _handler = new GetMPagoPorIdQueryHandler(
                _mockRepo.Object,
                _mockLog.Object
            );

            _idMpago = Guid.NewGuid().ToString();
            _idUsuario = Guid.NewGuid().ToString();
            _idMpagoStripe = "pm_123456789";
            _idClienteStripe = "cus_987654321";

            _mPagoEntity = new TarjetaCredito(
                idMPago: new VOIdMPago(_idMpago),
                idUsuario: new VOIdUsuario(_idUsuario),
                idMPagoStripe: new VOIdMPagoStripe(_idMpagoStripe),
                idClienteStripe: new VOIdClienteStripe(_idClienteStripe),
                marca: new VOMarca("visa"),
                mesExpiracion: new VOMesExpiracion(12),
                anioExpiracion: new VOAnioExpiracion(2030),
                ultimos4: new VOUltimos4("4242"),
                fechaRegistro: new VOFechaRegistro(DateTime.UtcNow),
                predeterminado: new VOPredeterminado(true)
            );
        }

        #region Handle_Success_ReturnsMappedDto
        [Fact]
        public async Task Handle_Success_ReturnsMappedDto()
        {
            // ARRANGE
            var query = new GetMPagoPorIdQuery(_idMpago);

            _mockRepo
                .Setup(r => r.ObtenerMPagoPorId(_idMpago))
                .ReturnsAsync(_mPagoEntity);

            // ACT
            var result = await _handler.Handle(query, CancellationToken.None);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(_idMpago, result.IdMPago);
            Assert.Equal(_idUsuario, result.IdUsuario);
            Assert.Equal(_idMpagoStripe, result.IdMPagoStripe);
            Assert.Equal(_idClienteStripe, result.IdClienteStripe);
            Assert.Equal("visa", result.Marca);
            Assert.Equal(12, result.MesExpiracion);
            Assert.Equal(2030, result.AnioExpiracion);
            Assert.Equal("4242", result.Ultimos4);
            Assert.True(result.Predeterminado);

            _mockRepo.Verify(r => r.ObtenerMPagoPorId(_idMpago), Times.Once);
        }
        #endregion

        #region Handle_NotFound_ReturnsEmptyDto
        [Fact]
        public async Task Handle_NotFound_ReturnsEmptyDto()
        {
            // ARRANGE
            var query = new GetMPagoPorIdQuery(_idMpago);

            _mockRepo
                .Setup(r => r.ObtenerMPagoPorId(_idMpago))
                .ReturnsAsync((TarjetaCredito)null!);

            // ACT
            var result = await _handler.Handle(query, CancellationToken.None);

            // ASSERT
            Assert.NotNull(result);
            // DTO vacío: todas las propiedades en sus valores por defecto
            Assert.Null(result.IdMPago);
            Assert.Null(result.IdUsuario);
            Assert.Null(result.IdMPagoStripe);
            Assert.Null(result.IdClienteStripe);
            Assert.Null(result.Marca);
            Assert.Equal(0, result.MesExpiracion);
            Assert.Equal(0, result.AnioExpiracion);
            Assert.Null(result.Ultimos4);
            Assert.Equal(default(DateTime), result.FechaRegistro);
            Assert.False(result.Predeterminado);

            _mockRepo.Verify(r => r.ObtenerMPagoPorId(_idMpago), Times.Once);
        }
        #endregion

        #region Handle_UnexpectedException_WrappedInGetMPagoPorIdQueryHandlerException
        [Fact]
        public async Task Handle_UnexpectedException_IsWrappedInGetMPagoPorIdQueryHandlerException()
        {
            // ARRANGE
            var query = new GetMPagoPorIdQuery(_idMpago);

            _mockRepo
                .Setup(r => r.ObtenerMPagoPorId(_idMpago))
                .ThrowsAsync(new Exception("Error inesperado en la DB"));

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<GetMPagoPorIdQueryHandlerException>(
                async () => await _handler.Handle(query, CancellationToken.None)
            );

            Assert.NotNull(ex.InnerException);
            Assert.Equal("Error inesperado en la DB", ex.InnerException!.Message);
        }
        #endregion
    }
}
