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
    public class GetPagosByIdUsuarioQueryHandler_Tests
    {
        private readonly Mock<IPagoRepository> _mockRepo;
        private readonly Mock<ILog> _mockLog;
        private readonly GetPagosByIdUsuarioQueryHandler _handler;

        private readonly string _idUsuario;
        private readonly string _idPago1;
        private readonly string _idPago2;
        private readonly string _idMPago1;
        private readonly string _idMPago2;
        private readonly string _idReserva1;
        private readonly string _idReserva2;
        private readonly string _idEvento1;
        private readonly string _idEvento2;

        private readonly Pago _pago1;
        private readonly Pago _pago2;

        public GetPagosByIdUsuarioQueryHandler_Tests()
        {
            _mockRepo = new Mock<IPagoRepository>();
            _mockLog = new Mock<ILog>();

            _handler = new GetPagosByIdUsuarioQueryHandler(
                _mockRepo.Object,
                _mockLog.Object
            );

            _idUsuario = Guid.NewGuid().ToString();

            _idPago1 = Guid.NewGuid().ToString();
            _idPago2 = Guid.NewGuid().ToString();

            _idMPago1 = Guid.NewGuid().ToString();
            _idMPago2 = Guid.NewGuid().ToString();

            _idReserva1 = Guid.NewGuid().ToString();
            _idReserva2 = Guid.NewGuid().ToString();

            _idEvento1 = Guid.NewGuid().ToString();
            _idEvento2 = Guid.NewGuid().ToString();

            _pago1 = new Pago(
                idPago: new VOIdPago(_idPago1),
                idMPago: new VOIdMPago(_idMPago1),
                idUsuario: new VOIdUsuario(_idUsuario),
                idReserva: new VOIdReserva(_idReserva1),
                idEvento: new VOIdEvento(_idEvento1),
                fechaPago: new VOFechaPago(DateTime.UtcNow.AddHours(-2)),
                monto: new VOMonto(50.75m),
                idExternalPago: new VOIdExternalPago("ext_001")
            );

            _pago2 = new Pago(
                idPago: new VOIdPago(_idPago2),
                idMPago: new VOIdMPago(_idMPago2),
                idUsuario: new VOIdUsuario(_idUsuario),
                idReserva: new VOIdReserva(_idReserva2),
                idEvento: new VOIdEvento(_idEvento2),
                fechaPago: new VOFechaPago(DateTime.UtcNow),
                monto: new VOMonto(99.99m),
                idExternalPago: new VOIdExternalPago("ext_002")
            );
        }

        #region Escenario exitoso
        [Fact]
        public async Task Handle_DeberiaRetornarListaMapeada_CuandoExistenPagosDelUsuario()
        {
            // ARRANGE
            var query = new GetPagosByIdUsuarioQuery(_idUsuario);

            _mockRepo
                .Setup(r => r.ObtenerPagosPorIdUsuario(_idUsuario))
                .ReturnsAsync(new List<Pago> { _pago1, _pago2 });

            // ACT
            var resultado = await _handler.Handle(query, CancellationToken.None);

            // ASSERT
            Assert.Equal(2, resultado.Count);

            var dto1 = resultado[0];
            Assert.Equal(_idPago1, dto1.IdPago);
            Assert.Equal(_idMPago1, dto1.IdMPago);
            Assert.Equal(_idUsuario, dto1.IdUsuario);
            Assert.Equal(_idReserva1, dto1.IdReserva);
            Assert.Equal(_idEvento1, dto1.IdEvento);
            Assert.Equal(50.75m, dto1.Monto);
            Assert.Equal("ext_001", dto1.IdExternalPago);

            var dto2 = resultado[1];
            Assert.Equal(_idPago2, dto2.IdPago);
            Assert.Equal(_idMPago2, dto2.IdMPago);
            Assert.Equal(_idUsuario, dto2.IdUsuario);
            Assert.Equal(_idReserva2, dto2.IdReserva);
            Assert.Equal(_idEvento2, dto2.IdEvento);
            Assert.Equal(99.99m, dto2.Monto);
            Assert.Equal("ext_002", dto2.IdExternalPago);

            _mockRepo.Verify(r => r.ObtenerPagosPorIdUsuario(_idUsuario), Times.Once);
        }
        #endregion

        #region Escenario sin registros
        [Fact]
        public async Task Handle_DeberiaRetornarListaVacia_CuandoUsuarioNoTienePagos()
        {
            // ARRANGE
            var query = new GetPagosByIdUsuarioQuery(_idUsuario);

            _mockRepo
                .Setup(r => r.ObtenerPagosPorIdUsuario(_idUsuario))
                .ReturnsAsync(new List<Pago>()); // Lista vacía

            // ACT
            var resultado = await _handler.Handle(query, CancellationToken.None);

            // ASSERT
            Assert.Empty(resultado);
            _mockRepo.Verify(r => r.ObtenerPagosPorIdUsuario(_idUsuario), Times.Once);
        }
        #endregion

        #region Escenario error inesperado
        [Fact]
        public async Task Handle_DeberiaLanzarExcepcionCustom_CuandoOcurreErrorInesperado()
        {
            // ARRANGE
            var query = new GetPagosByIdUsuarioQuery(_idUsuario);

            _mockRepo
                .Setup(r => r.ObtenerPagosPorIdUsuario(_idUsuario))
                .ThrowsAsync(new Exception("Error interno"));

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<GetPagosUsuarioQueryHandlerException>(
                async () => await _handler.Handle(query, CancellationToken.None)
            );

            Assert.Equal("Error interno", ex.InnerException!.Message);
        }
        #endregion
    }
}
