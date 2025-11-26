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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pagos.Tests.Pagos.API.Controller
{
    public class PagosController_GetPagoPorIdEvento_Tests
    {
        private readonly Mock<IMediator> MockMediator;
        private readonly Mock<IRestClient> MockClient;
        private readonly Mock<ILog> MockLogger;
        private readonly PagosController Controller;

        // --- DATOS ---
        private readonly string TestEventoId;
        private readonly List<PagoDTO> ListaPagos;

        public PagosController_GetPagoPorIdEvento_Tests()
        {
            MockMediator = new Mock<IMediator>();
            MockClient = new Mock<IRestClient>();
            MockLogger = new Mock<ILog>();

            Controller = new PagosController(
                MockMediator.Object,
                MockClient.Object,
                MockLogger.Object
            );

            TestEventoId = "evento-1";

            ListaPagos = new List<PagoDTO>
            {
                new PagoDTO
                {
                    IdPago    = "pago-1",
                    IdUsuario = "usuario-1",
                    IdReserva = "reserva-1",
                    IdEvento  = TestEventoId,
                    Monto     = 100m
                },
                new PagoDTO
                {
                    IdPago    = "pago-2",
                    IdUsuario = "usuario-2",
                    IdReserva = "reserva-2",
                    IdEvento  = TestEventoId,
                    Monto     = 150m
                }
            };
        }


        #region GetPagoPorIdEvento_ExistenPagos_Retorna200Ok()
        [Fact]
        public async Task GetPagoPorIdEvento_ExistenPagos_Retorna200Ok()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<GetPagosByIdEventoQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(ListaPagos);

            // ACT
            var result = await Controller.GetPagoPorIdEvento(TestEventoId);

            // ASSERT
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);

            var body = Assert.IsAssignableFrom<IEnumerable<PagoDTO>>(ok.Value);
            Assert.Equal(2, body.Count());

            MockMediator.Verify(m => m.Send(
                    It.Is<GetPagosByIdEventoQuery>(q => q.IdEvento == TestEventoId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion


        #region GetPagoPorIdEvento_PagosEsNull_Retorna404NotFound()
        [Fact]
        public async Task GetPagoPorIdEvento_PagosEsNull_Retorna404NotFound()
        {
            // ARRANGE
            var noPaymentsEventId = "evento-sin-pagos";

            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<GetPagosByIdEventoQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<PagoDTO>?)null);

            // ACT
            var result = await Controller.GetPagoPorIdEvento(noPaymentsEventId);

            // ASSERT
            var nf = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, nf.StatusCode);
            Assert.Equal($"No se encontraron pagos con el id {noPaymentsEventId}", nf.Value);

            MockMediator.Verify(m => m.Send(
                    It.Is<GetPagosByIdEventoQuery>(q => q.IdEvento == noPaymentsEventId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion


        #region GetPagoPorIdEvento_LanzaExcepcion_Retorna500InternalServerError()
        [Fact]
        public async Task GetPagoPorIdEvento_LanzaExcepcion_Retorna500InternalServerError()
        {
            // ARRANGE
            var expectedException = new Exception("Error de prueba");

            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<GetPagosByIdEventoQuery>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // ACT
            var result = await Controller.GetPagoPorIdEvento(TestEventoId);

            // ASSERT
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
            Assert.Equal(expectedException.Message, obj.Value);

            MockMediator.Verify(m => m.Send(
                    It.Is<GetPagosByIdEventoQuery>(q => q.IdEvento == TestEventoId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion
    }
}
