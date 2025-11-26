using log4net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pagos.API.Controllers;
using Pagos.Application.DTOs;
using Pagos.Infrastructure.Queries;
using RestSharp;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pagos.Tests.Pagos.API.Controller
{
    public class PagosController_GetPagoPorId_Tests
    {
        private readonly Mock<IMediator> MockMediator;
        private readonly Mock<IRestClient> MockClient;
        private readonly Mock<ILog> MockLogger;
        private readonly PagosController Controller;

        // --- DATOS ---
        private readonly string TestPagoId;
        private readonly PagoDTO ValidPago;

        public PagosController_GetPagoPorId_Tests()
        {
            MockMediator = new Mock<IMediator>();
            MockClient = new Mock<IRestClient>();
            MockLogger = new Mock<ILog>();

            Controller = new PagosController(
                MockMediator.Object,
                MockClient.Object,
                MockLogger.Object
            );

            TestPagoId = "pago-1";

            ValidPago = new PagoDTO
            {
                IdPago = TestPagoId,
                IdUsuario = "usuario-1",
                IdReserva = "reserva-1",
                Monto = 100m
            };
        }


        #region GetPagoPorId_PagoExiste_Retorna200Ok()
        [Fact]
        public async Task GetPagoPorId_PagoExiste_Retorna200Ok()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<GetPagoPorIdQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidPago);

            // ACT
            var result = await Controller.GetPagoPorId(TestPagoId);

            // ASSERT
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);

            var body = Assert.IsType<PagoDTO>(ok.Value);
            Assert.Equal(TestPagoId, body.IdPago);

            MockMediator.Verify(m => m.Send(
                    It.Is<GetPagoPorIdQuery>(q => q.IdPago == TestPagoId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion


        #region GetPagoPorId_PagoEsNull_Retorna404NotFound()
        [Fact]
        public async Task GetPagoPorId_PagoEsNull_Retorna404NotFound()
        {
            // ARRANGE
            var nonExistingId = "pago-no-existe";

            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<GetPagoPorIdQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((PagoDTO?)null);

            // ACT
            var result = await Controller.GetPagoPorId(nonExistingId);

            // ASSERT
            var nf = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, nf.StatusCode);
            Assert.Equal($"No se encontró un pago con el id {nonExistingId}", nf.Value);

            MockMediator.Verify(m => m.Send(
                    It.Is<GetPagoPorIdQuery>(q => q.IdPago == nonExistingId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion



        #region GetPagoPorId_LanzaExcepcion_Retorna500InternalServerError()
        [Fact]
        public async Task GetPagoPorId_LanzaExcepcion_Retorna500InternalServerError()
        {
            // ARRANGE
            var expectedException = new Exception("Error de prueba");

            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<GetPagoPorIdQuery>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // ACT
            var result = await Controller.GetPagoPorId(TestPagoId);

            // ASSERT
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
            Assert.Equal(expectedException.Message, obj.Value);

            MockMediator.Verify(m => m.Send(
                    It.Is<GetPagoPorIdQuery>(q => q.IdPago == TestPagoId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion
    }
}
