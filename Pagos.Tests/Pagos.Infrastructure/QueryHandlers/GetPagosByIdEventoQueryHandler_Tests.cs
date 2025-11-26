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
    public class GetPagosByIdEventoQueryHandler_Tests
    {
        private readonly Mock<IPagoRepository> _mockRepo;
        private readonly Mock<ILog> _mockLog;
        private readonly GetPagosByIdEventoQueryHandler _handler;

        private readonly string _idEvento;
        private readonly string _idPago1;
        private readonly string _idPago2;
        private readonly string _idMPago1;
        private readonly string _idMPago2;
        private readonly string _idUsuario1;
        private readonly string _idUsuario2;
        private readonly string _idReserva1;
        private readonly string _idReserva2;

        private readonly Pago _pago1;
        private readonly Pago _pago2;

        public GetPagosByIdEventoQueryHandler_Tests()
        {
            _mockRepo = new Mock<IPagoRepository>();
            _mockLog = new Mock<ILog>();

            _handler = new GetPagosByIdEventoQueryHandler(
                _mockRepo.Object,
                _mockLog.Object
            );

            _idEvento = Guid.NewGuid().ToString();
            _idPago1 = Guid.NewGuid().ToString();
            _idPago2 = Guid.NewGuid().ToString();
            _idMPago1 = Guid.NewGuid().ToString();
            _idMPago2 = Guid.NewGuid().ToString();
            _idUsuario1 = Guid.NewGuid().ToString();
            _idUsuario2 = Guid.NewGuid().ToString();
            _idReserva1 = Guid.NewGuid().ToString();
            _idReserva2 = Guid.NewGuid().ToString();

            _pago1 = new Pago(
                idPago: new VOIdPago(_idPago1),
                idMPago: new VOIdMPago(_idMPago1),
                idUsuario: new VOIdUsuario(_idUsuario1),
                idReserva: new VOIdReserva(_idReserva1),
                idEvento: new VOIdEvento(_idEvento),
                fechaPago: new VOFechaPago(DateTime.UtcNow.AddDays(-1)),
                monto: new VOMonto(100.50m),
                idExternalPago: new VOIdExternalPago("ext_111")
            );

            _pago2 = new Pago(
                idPago: new VOIdPago(_idPago2),
                idMPago: new VOIdMPago(_idMPago2),
                idUsuario: new VOIdUsuario(_idUsuario2),
                idReserva: new VOIdReserva(_idReserva2),
                idEvento: new VOIdEvento(_idEvento),
                fechaPago: new VOFechaPago(DateTime.UtcNow),
                monto: new VOMonto(200.75m),
                idExternalPago: new VOIdExternalPago("ext_222")
            );
        }

        #region Escenario exitoso: devuelve lista mapeada
        [Fact]
        public async Task Handle_DeberiaDevolverListaMapeada_CuandoHayPagosParaElEvento()
        {
            // ARRANGE
            var query = new GetPagosByIdEventoQuery(_idEvento);

            _mockRepo
                .Setup(r => r.ObtenerPagosPorIdEvento(_idEvento))
                .ReturnsAsync(new List<Pago> { _pago1, _pago2 });

            // ACT
            var resultado = await _handler.Handle(query, CancellationToken.None);

            // ASSERT
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Count);

            // Pago 1
            var dto1 = resultado[0];
            Assert.Equal(_idPago1, dto1.IdPago);
            Assert.Equal(_idMPago1, dto1.IdMPago);
            Assert.Equal(_idUsuario1, dto1.IdUsuario);
            Assert.Equal(_idReserva1, dto1.IdReserva);
            Assert.Equal(_idEvento, dto1.IdEvento);
            Assert.Equal(100.50m, dto1.Monto);
            Assert.Equal("ext_111", dto1.IdExternalPago);

            // Pago 2
            var dto2 = resultado[1];
            Assert.Equal(_idPago2, dto2.IdPago);
            Assert.Equal(_idMPago2, dto2.IdMPago);
            Assert.Equal(_idUsuario2, dto2.IdUsuario);
            Assert.Equal(_idReserva2, dto2.IdReserva);
            Assert.Equal(_idEvento, dto2.IdEvento);
            Assert.Equal(200.75m, dto2.Monto);
            Assert.Equal("ext_222", dto2.IdExternalPago);

            _mockRepo.Verify(r => r.ObtenerPagosPorIdEvento(_idEvento), Times.Once);
        }
        #endregion

        #region Escenario sin datos: devuelve lista vacía
        [Fact]
        public async Task Handle_DeberiaDevolverListaVacia_CuandoNoHayPagosParaElEvento()
        {
            // ARRANGE
            var query = new GetPagosByIdEventoQuery(_idEvento);

            _mockRepo
                .Setup(r => r.ObtenerPagosPorIdEvento(_idEvento))
                .ReturnsAsync(new List<Pago>());

            // ACT
            var resultado = await _handler.Handle(query, CancellationToken.None);

            // ASSERT
            Assert.NotNull(resultado);
            Assert.Empty(resultado);

            _mockRepo.Verify(r => r.ObtenerPagosPorIdEvento(_idEvento), Times.Once);
        }
        #endregion

        #region Escenario de error: excepción inesperada envuelta
        [Fact]
        public async Task Handle_DeberiaLanzarGetPagosByIdEventoQueryHandlerException_CuandoOcurreErrorInesperado()
        {
            // ARRANGE
            var query = new GetPagosByIdEventoQuery(_idEvento);

            _mockRepo
                .Setup(r => r.ObtenerPagosPorIdEvento(_idEvento))
                .ThrowsAsync(new Exception("Error en la base de datos"));

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<GetPagosByIdEventoQueryHandlerException>(
                async () => await _handler.Handle(query, CancellationToken.None)
            );

            Assert.NotNull(ex.InnerException);
            Assert.Equal("Error en la base de datos", ex.InnerException!.Message);
        }
        #endregion
    }
}
