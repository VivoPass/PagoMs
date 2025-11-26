using log4net;
using Moq;
using Pagos.Application.DTOs;
using Pagos.Domain.Aggregates;
using Pagos.Domain.Entities;
using Pagos.Domain.Exceptions;
using Pagos.Domain.Interfaces;
using Pagos.Domain.ValueObjects;
using Pagos.Infrastructure.Queries;
using Pagos.Infrastructure.Queries.QueryHandlers;
using Xunit;

namespace Pagos.Tests.Pagos.Infrastructure.QueryHandlers
{
    public class GetPagoPorIdQueryHandler_Tests
    {
        private readonly Mock<IPagoRepository> _mockRepo;
        private readonly Mock<ILog> _mockLog;
        private readonly GetPagoPorIdQueryHandler _handler;

        private readonly string _idPago;
        private readonly string _idMPago;
        private readonly string _idUsuario;
        private readonly string _idReserva;
        private readonly string _idEvento;

        private readonly Pago _pagoEntity;

        public GetPagoPorIdQueryHandler_Tests()
        {
            _mockRepo = new Mock<IPagoRepository>();
            _mockLog = new Mock<ILog>();

            _handler = new GetPagoPorIdQueryHandler(
                _mockRepo.Object,
                _mockLog.Object
            );

            _idPago = Guid.NewGuid().ToString();
            _idMPago = Guid.NewGuid().ToString();
            _idUsuario = Guid.NewGuid().ToString();
            _idReserva = Guid.NewGuid().ToString();
            _idEvento = Guid.NewGuid().ToString();

            _pagoEntity = new Pago(
                idPago: new VOIdPago(_idPago),
                idMPago: new VOIdMPago(_idMPago),
                idUsuario: new VOIdUsuario(_idUsuario),
                idReserva: new VOIdReserva(_idReserva),
                idEvento: new VOIdEvento(_idEvento),
                fechaPago: new VOFechaPago(DateTime.UtcNow),
                monto: new VOMonto(150),
                idExternalPago: new VOIdExternalPago("stripe_123456")
            );
        }

        #region Handle_Success_ReturnsDto
        [Fact]
        public async Task Handle_Success_ReturnsDto()
        {
            // ARRANGE
            var query = new GetPagoPorIdQuery(_idPago);

            _mockRepo
                .Setup(r => r.ObtenerPagoPorIdPago(_idPago))
                .ReturnsAsync(_pagoEntity);

            // ACT
            var result = await _handler.Handle(query, CancellationToken.None);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(_idPago, result!.IdPago);
            Assert.Equal(_idMPago, result.IdMPago);
            Assert.Equal(_idUsuario, result.IdUsuario);
            Assert.Equal(_idReserva, result.IdReserva);
            Assert.Equal(_idEvento, result.IdEvento);
            Assert.Equal(150, result.Monto);
            Assert.Equal("stripe_123456", result.IdExternalPago);

            _mockRepo.Verify(r => r.ObtenerPagoPorIdPago(_idPago), Times.Once);
        }
        #endregion

        #region Handle_NotFound_ReturnsNull
        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            var query = new GetPagoPorIdQuery(_idPago);

            _mockRepo
                .Setup(r => r.ObtenerPagoPorIdPago(_idPago))
                .ReturnsAsync((Pago?)null);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.Null(result);

            _mockRepo.Verify(r => r.ObtenerPagoPorIdPago(_idPago), Times.Once);
        }
        #endregion

        #region Handle_UnexpectedException_ThrowsWrappedException
        [Fact]
        public async Task Handle_UnexpectedException_ThrowsWrappedException()
        {
            var query = new GetPagoPorIdQuery(_idPago);

            _mockRepo
                .Setup(r => r.ObtenerPagoPorIdPago(_idPago))
                .ThrowsAsync(new Exception("DB exploded"));

            var ex = await Assert.ThrowsAsync<GetPagoPorIdQueryHandlerException>(async () =>
                await _handler.Handle(query, CancellationToken.None)
            );

            Assert.NotNull(ex.InnerException);
            Assert.Equal("DB exploded", ex.InnerException!.Message);
        }
        #endregion
    }
}
