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
    public class GetTodosMPagoQueryHandler_Tests
    {
        private readonly Mock<IMPagoRepository> _mockRepo;
        private readonly Mock<ILog> _mockLog;
        private readonly GetTodosMPagoQueryHandler _handler;

        private readonly string _idMpago1;
        private readonly string _idMpago2;
        private readonly string _idUsuario1;
        private readonly string _idUsuario2;
        private readonly string _idMpagoStripe1;
        private readonly string _idMpagoStripe2;
        private readonly string _idClienteStripe1;
        private readonly string _idClienteStripe2;

        private readonly TarjetaCredito _mpago1;
        private readonly TarjetaCredito _mpago2;

        public GetTodosMPagoQueryHandler_Tests()
        {
            _mockRepo = new Mock<IMPagoRepository>();
            _mockLog = new Mock<ILog>();

            _handler = new GetTodosMPagoQueryHandler(
                _mockRepo.Object,
                _mockLog.Object
            );

            _idMpago1 = Guid.NewGuid().ToString();
            _idMpago2 = Guid.NewGuid().ToString();
            _idUsuario1 = Guid.NewGuid().ToString();
            _idUsuario2 = Guid.NewGuid().ToString();
            _idMpagoStripe1 = "pm_111111";
            _idMpagoStripe2 = "pm_222222";
            _idClienteStripe1 = "cus_111";
            _idClienteStripe2 = "cus_222";

            _mpago1 = new TarjetaCredito(
                idMPago: new VOIdMPago(_idMpago1),
                idUsuario: new VOIdUsuario(_idUsuario1),
                idMPagoStripe: new VOIdMPagoStripe(_idMpagoStripe1),
                idClienteStripe: new VOIdClienteStripe(_idClienteStripe1),
                marca: new VOMarca("Visa"),
                mesExpiracion: new VOMesExpiracion(12),
                anioExpiracion: new VOAnioExpiracion(2030),
                ultimos4: new VOUltimos4("4242"),
                fechaRegistro: new VOFechaRegistro(DateTime.UtcNow.AddDays(-1)),
                predeterminado: new VOPredeterminado(true)
            );

            _mpago2 = new TarjetaCredito(
                idMPago: new VOIdMPago(_idMpago2),
                idUsuario: new VOIdUsuario(_idUsuario2),
                idMPagoStripe: new VOIdMPagoStripe(_idMpagoStripe2),
                idClienteStripe: new VOIdClienteStripe(_idClienteStripe2),
                marca: new VOMarca("Mastercard"),
                mesExpiracion: new VOMesExpiracion(6),
                anioExpiracion: new VOAnioExpiracion(2028),
                ultimos4: new VOUltimos4("1111"),
                fechaRegistro: new VOFechaRegistro(DateTime.UtcNow),
                predeterminado: new VOPredeterminado(false)
            );
        }

        #region Escenario exitoso: lista con datos
        [Fact]
        public async Task Handle_DeberiaRetornarListaMapeada_CuandoRepositorioRetornaDatos()
        {
            // ARRANGE
            var query = new GetTodosMPagoQuery();

            _mockRepo
                .Setup(r => r.GetTodosMPago())
                .ReturnsAsync(new List<TarjetaCredito> { _mpago1, _mpago2 });

            // ACT
            var resultado = await _handler.Handle(query, CancellationToken.None);

            // ASSERT
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Count);

            var dto1 = resultado[0];
            Assert.Equal(_idMpago1, dto1.IdMPago);
            Assert.Equal(_idUsuario1, dto1.IdUsuario);
            Assert.Equal(_idMpagoStripe1, dto1.IdMPagoStripe);
            Assert.Equal(_idClienteStripe1, dto1.IdClienteStripe);
            Assert.Equal("Visa", dto1.Marca);
            Assert.Equal(12, dto1.MesExpiracion);
            Assert.Equal(2030, dto1.AnioExpiracion);
            Assert.Equal("4242", dto1.Ultimos4);
            Assert.True(dto1.Predeterminado);

            var dto2 = resultado[1];
            Assert.Equal(_idMpago2, dto2.IdMPago);
            Assert.Equal(_idUsuario2, dto2.IdUsuario);
            Assert.Equal(_idMpagoStripe2, dto2.IdMPagoStripe);
            Assert.Equal(_idClienteStripe2, dto2.IdClienteStripe);
            Assert.Equal("Mastercard", dto2.Marca);
            Assert.Equal(6, dto2.MesExpiracion);
            Assert.Equal(2028, dto2.AnioExpiracion);
            Assert.Equal("1111", dto2.Ultimos4);
            Assert.False(dto2.Predeterminado);

            _mockRepo.Verify(r => r.GetTodosMPago(), Times.Once);
        }
        #endregion

        #region Escenario error de negocio: lista nula o vacía
        [Fact]
        public async Task Handle_DeberiaLanzarMPagoNullException_CuandoRepositorioRetornaNull()
        {
            var query = new GetTodosMPagoQuery();

            _mockRepo
                .Setup(r => r.GetTodosMPago())
                .ReturnsAsync((List<TarjetaCredito>?)null);

            await Assert.ThrowsAsync<MPagoNullException>(
                async () => await _handler.Handle(query, CancellationToken.None)
            );

            _mockRepo.Verify(r => r.GetTodosMPago(), Times.Once);
        }

        [Fact]
        public async Task Handle_DeberiaLanzarMPagoNullException_CuandoRepositorioRetornaListaVacia()
        {
            var query = new GetTodosMPagoQuery();

            _mockRepo
                .Setup(r => r.GetTodosMPago())
                .ReturnsAsync(new List<TarjetaCredito>());

            await Assert.ThrowsAsync<MPagoNullException>(
                async () => await _handler.Handle(query, CancellationToken.None)
            );

            _mockRepo.Verify(r => r.GetTodosMPago(), Times.Once);
        }
        #endregion

        #region Escenario error inesperado: excepción envuelta
        [Fact]
        public async Task Handle_DeberiaLanzarGetTodosMPagoQueryHandlerException_CuandoOcurreErrorInesperado()
        {
            var query = new GetTodosMPagoQuery();

            _mockRepo
                .Setup(r => r.GetTodosMPago())
                .ThrowsAsync(new Exception("Fallo de conexión"));

            var ex = await Assert.ThrowsAsync<GetTodosMPagoQueryHandlerException>(
                async () => await _handler.Handle(query, CancellationToken.None)
            );

            Assert.NotNull(ex.InnerException);
            Assert.Equal("Fallo de conexión", ex.InnerException!.Message);
        }
        #endregion
    }
}
