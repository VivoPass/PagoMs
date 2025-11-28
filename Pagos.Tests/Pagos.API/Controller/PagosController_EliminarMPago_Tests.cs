using log4net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pagos.API.Controllers;
using Pagos.Application.Commands;
using Pagos.Application.DTOs;
using Pagos.Infrastructure.Queries;
using RestSharp;
using System.Net;
using Xunit;

namespace Pagos.Tests.Pagos.API.Controller
{
    public class PagosController_EliminarMPago_Tests
    {
        private readonly Mock<IMediator> MockMediator;
        private readonly Mock<IRestClient> MockClient;
        private readonly Mock<ILog> MockLogger;
        private readonly PagosController Controller;

        // --- DATOS ---
        private readonly string TestMPagoId;
        private readonly string TestUsuarioId;
        private readonly MPagoDTO ValidMPago;

        public PagosController_EliminarMPago_Tests()
        {
            MockMediator = new Mock<IMediator>();
            MockClient = new Mock<IRestClient>();
            MockLogger = new Mock<ILog>();

            Controller = new PagosController(
                MockMediator.Object,
                MockClient.Object,
                MockLogger.Object
            );

            TestMPagoId = "pm_1SWlTHRKEQAOXjwpkyXRW7xA";
            TestUsuarioId = "usr_test";

            ValidMPago = new MPagoDTO
            {
                IdMPago = TestMPagoId,
                IdUsuario = TestUsuarioId,
                Ultimos4 = "4242"
            };
        }



        #region EliminarMPago_TodoExitoso_Retorna200Ok()
        [Fact]
        public async Task EliminarMPago_TodoEsExitoso_Retorna200Ok()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<EliminarMPagoCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<GetMPagoPorIdQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidMPago);

            MockClient
                .Setup(r => r.ExecuteAsync(
                    It.IsAny<RestRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RestResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseStatus = ResponseStatus.Completed,
                    IsSuccessStatusCode = true
                });

            // ACT
            var result = await Controller.EliminarMPago(TestMPagoId);

            // ASSERT
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
            Assert.Equal("MPago eliminado exitosamente.", ok.Value);

            // Verificamos llamadas clave
            MockMediator.Verify(m => m.Send(
                    It.IsAny<EliminarMPagoCommand>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            MockMediator.Verify(m => m.Send(
                    It.IsAny<GetMPagoPorIdQuery>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            MockClient.Verify(r => r.ExecuteAsync(
                    It.IsAny<RestRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region EliminarMPago_TodoEsExitoso_InvocaCommandQueryYMSUsuarios()
        [Fact]
        public async Task EliminarMPago_TodoEsExitoso_InvocaCommandQueryYMSUsuarios()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<EliminarMPagoCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<GetMPagoPorIdQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidMPago);

            MockClient
                .Setup(c => c.ExecuteAsync(
                    It.IsAny<RestRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RestResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseStatus = ResponseStatus.Completed
                });

            // ACT
            await Controller.EliminarMPago(TestMPagoId);

            // ASSERT
            MockMediator.Verify(m => m.Send(
                    It.IsAny<EliminarMPagoCommand>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            MockMediator.Verify(m => m.Send(
                    It.IsAny<GetMPagoPorIdQuery>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            MockClient.Verify(c => c.ExecuteAsync(
                    It.IsAny<RestRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion



        #region EliminarMPago_CommandDevuelveFalse_Retorna404NotFound()
        [Fact]
        public async Task EliminarMPago_CommandDevuelveFalse_Retorna404NotFound()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<EliminarMPagoCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // ACT
            var result = await Controller.EliminarMPago(TestMPagoId);

            // ASSERT
            var nf = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, nf.StatusCode);
            Assert.Equal("El MPago no pudo ser eliminado.", nf.Value);

            // No debe llamar MS Usuarios

            MockClient.Verify(c => c.ExecuteAsync(
                    It.IsAny<RestRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion

        #region EliminarMPago_MsUsuariosFalla_Retorna500InternalServerError()
        [Fact]
        public async Task EliminarMPago_MsUsuariosFalla_Retorna500InternalServerError()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<EliminarMPagoCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<GetMPagoPorIdQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidMPago);

            MockClient
                .Setup(r => r.ExecuteAsync(
                    It.IsAny<RestRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RestResponse
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ResponseStatus = ResponseStatus.Error,
                    IsSuccessStatusCode = false
                });

            // ACT
            var result = await Controller.EliminarMPago(TestMPagoId);

            // ASSERT
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
            Assert.Equal("Error al completar la publicación de la actividad del usuario", obj.Value);
        }
        #endregion

        #region EliminarMPago_LanzaExcepcion_Retorna500InternalServerError()
        [Fact]
        public async Task EliminarMPago_LanzaExcepcion_Retorna500InternalServerError()
        {
            // ARRANGE
            var expectedException = new Exception("Error inesperado");

            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<EliminarMPagoCommand>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // ACT
            var result = await Controller.EliminarMPago(TestMPagoId);

            // ASSERT
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
            Assert.NotNull(statusCodeResult.Value);

            // No debe llamar MS Usuarios

            MockClient.Verify(c => c.ExecuteAsync(
                    It.IsAny<RestRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion
    }
}
