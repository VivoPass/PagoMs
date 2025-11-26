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
    public class GetMPagoPorIdUsuarioQueryHandler_Tests
    {
        private readonly Mock<IMPagoRepository> _mockRepo;
        private readonly Mock<ILog> _mockLog;
        private readonly GetMPagoPorIdUsuarioQueryHandler _handler;

        private readonly string _idUsuario;
        private readonly string _idMpago1;
        private readonly string _idMpago2;
        private readonly TarjetaCredito _mpago1;
        private readonly TarjetaCredito _mpago2;

        public GetMPagoPorIdUsuarioQueryHandler_Tests()
        {
            _mockRepo = new Mock<IMPagoRepository>();
            _mockLog = new Mock<ILog>();

            _handler = new GetMPagoPorIdUsuarioQueryHandler(
                _mockRepo.Object,
                _mockLog.Object
            );

            _idUsuario = Guid.NewGuid().ToString();
            _idMpago1 = Guid.NewGuid().ToString();
            _idMpago2 = Guid.NewGuid().ToString();

            _mpago1 = new TarjetaCredito(
                idMPago: new VOIdMPago(_idMpago1),
                idUsuario: new VOIdUsuario(_idUsuario),
                idMPagoStripe: new VOIdMPagoStripe("pm_111"),
                idClienteStripe: new VOIdClienteStripe("cus_111"),
                marca: new VOMarca("visa"),
                mesExpiracion: new VOMesExpiracion(12),
                anioExpiracion: new VOAnioExpiracion(2030),
                ultimos4: new VOUltimos4("4242"),
                fechaRegistro: new VOFechaRegistro(DateTime.UtcNow),
                predeterminado: new VOPredeterminado(true)
            );

            _mpago2 = new TarjetaCredito(
                idMPago: new VOIdMPago(_idMpago2),
                idUsuario: new VOIdUsuario(_idUsuario),
                idMPagoStripe: new VOIdMPagoStripe("pm_222"),
                idClienteStripe: new VOIdClienteStripe("cus_222"),
                marca: new VOMarca("mastercard"),
                mesExpiracion: new VOMesExpiracion(3),
                anioExpiracion: new VOAnioExpiracion(2029),
                ultimos4: new VOUltimos4("1111"),
                fechaRegistro: new VOFechaRegistro(DateTime.UtcNow),
                predeterminado: new VOPredeterminado(false)
            );
        }

        #region Handle_Success_ReturnsMappedDtoList
        [Fact]
        public async Task Handle_Success_ReturnsMappedDtoList()
        {
            // ARRANGE
            var query = new GetMPagoPorIdUsuarioQuery(_idUsuario);

            var lista = new List<TarjetaCredito> { _mpago1, _mpago2 };

            _mockRepo
                .Setup(r => r.ObtenerMPagoPorIdUsuario(_idUsuario))
                .ReturnsAsync(lista);

            // ACT
            var result = await _handler.Handle(query, CancellationToken.None);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            // MPAGO 1
            Assert.Equal(_idMpago1, result[0].IdMPago);
            Assert.Equal("visa", result[0].Marca);
            Assert.Equal("4242", result[0].Ultimos4);
            Assert.True(result[0].Predeterminado);

            // MPAGO 2
            Assert.Equal(_idMpago2, result[1].IdMPago);
            Assert.Equal("mastercard", result[1].Marca);
            Assert.Equal("1111", result[1].Ultimos4);
            Assert.False(result[1].Predeterminado);

            _mockRepo.Verify(r => r.ObtenerMPagoPorIdUsuario(_idUsuario), Times.Once);
        }
        #endregion

        #region Handle_NoRecords_ReturnsEmptyList
        [Fact]
        public async Task Handle_NoRecords_ReturnsEmptyList()
        {
            var query = new GetMPagoPorIdUsuarioQuery(_idUsuario);

            _mockRepo
                .Setup(r => r.ObtenerMPagoPorIdUsuario(_idUsuario))
                .ReturnsAsync(new List<TarjetaCredito>());

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result);

            _mockRepo.Verify(r => r.ObtenerMPagoPorIdUsuario(_idUsuario), Times.Once);
        }
        #endregion

        #region Handle_UnexpectedException_ThrowsWrappedException
        [Fact]
        public async Task Handle_UnexpectedException_ThrowsWrappedException()
        {
            var query = new GetMPagoPorIdUsuarioQuery(_idUsuario);

            _mockRepo
                .Setup(r => r.ObtenerMPagoPorIdUsuario(_idUsuario))
                .ThrowsAsync(new Exception("DB error"));

            var ex = await Assert.ThrowsAsync<GetMPagoPorIdPostorQueryHandlerException>(
                async () => await _handler.Handle(query, CancellationToken.None)
            );

            Assert.NotNull(ex.InnerException);
            Assert.Equal("DB error", ex.InnerException!.Message);
        }
        #endregion
    }
}
