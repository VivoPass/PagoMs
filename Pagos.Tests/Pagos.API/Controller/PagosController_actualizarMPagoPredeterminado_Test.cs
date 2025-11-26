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

namespace Pagos.Tests.Pagos.API.Controller
{
    public class PagosController_ActualizarMPagoPredeterminado_Tests
    {
        private readonly Mock<IMediator> MockMediator;
        private readonly Mock<IRestClient> MockClient;
        private readonly Mock<ILog> MockLogger;
        private readonly PagosController Controller;

        // --- DATOS ---
        private readonly string TestMPagoId;
        private readonly string TestUsuarioId;
        private readonly MPagoDTO ValidMPago;

        public PagosController_ActualizarMPagoPredeterminado_Tests()
        {
            MockMediator = new Mock<IMediator>();
            MockClient = new Mock<IRestClient>();
            MockLogger = new Mock<ILog>();

            Controller = new PagosController(
                MockMediator.Object,
                MockClient.Object,
                MockLogger.Object
            );

            TestMPagoId = "mpago_123";
            TestUsuarioId = "usr_123";

            ValidMPago = new MPagoDTO
            {
                IdMPago = TestMPagoId,
                IdUsuario = TestUsuarioId,
                Ultimos4 = "4242",
                Marca = "Visa",
                MesExpiracion = 12,
                AnioExpiracion = 2030,
                Predeterminado = true
            };
        }



        #region ActualizarMPagoPredeterminado_TodoExitoso_Retorna200Ok()
        [Fact]
        public async Task ActualizarMPagoPredeterminado_TodoExitoso_Retorna200Ok()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<MPagoPredeterminadoCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<GetMPagoPorIdQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidMPago);

            var successfulResponse = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed,
                IsSuccessStatusCode = true,
                Content = "OK"
            };

            MockClient
                .Setup(c => c.ExecuteAsync(
                    It.IsAny<RestRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(successfulResponse);

            // ACT
            var result = await Controller.ActualizarMPagoPredeterminado(TestMPagoId, TestUsuarioId);

            // ASSERT
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
            Assert.Equal("MPago actualizado a predeterminado exitosamente.", ok.Value);
        }
        #endregion

        #region ActualizarMPagoPredeterminado_TodoExitoso_InvocaCommandQueryYMSUsuarios()
        [Fact]
        public async Task ActualizarMPagoPredeterminado_TodoExitoso_InvocaCommandQueryYMSUsuarios()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<MPagoPredeterminadoCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<GetMPagoPorIdQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidMPago);

            var successfulResponse = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed,
                IsSuccessStatusCode = true,
                Content = "OK"
            };

            MockClient
                .Setup(c => c.ExecuteAsync(
                    It.IsAny<RestRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(successfulResponse);

            // ACT
            await Controller.ActualizarMPagoPredeterminado(TestMPagoId, TestUsuarioId);

            // ASSERT
            MockMediator.Verify(m => m.Send(
                    It.IsAny<MPagoPredeterminadoCommand>(),
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



        #region ActualizarMPagoPredeterminado_CommandDevuelveFalse_Retorna404NotFound()
        [Fact]
        public async Task ActualizarMPagoPredeterminado_CommandDevuelveFalse_Retorna404NotFound()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<MPagoPredeterminadoCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // ACT
            var result = await Controller.ActualizarMPagoPredeterminado(TestMPagoId, TestUsuarioId);

            // ASSERT
            var nf = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, nf.StatusCode);
            Assert.Equal("El MPago no pudo ser actualizado a predeterminado.", nf.Value);

            // No se debe llamar ni al query ni al MS Usuarios
            MockMediator.Verify(m => m.Send(
                    It.IsAny<GetMPagoPorIdQuery>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            MockClient.Verify(c => c.ExecuteAsync(
                    It.IsAny<RestRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion

        #region ActualizarMPagoPredeterminado_MsUsuariosFalla_Retorna500InternalServerError()
        [Fact]
        public async Task ActualizarMPagoPredeterminado_MsUsuariosFalla_Retorna500InternalServerError()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<MPagoPredeterminadoCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<GetMPagoPorIdQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidMPago);

            var failedResponse = new RestResponse
            {
                StatusCode = HttpStatusCode.InternalServerError,
                ResponseStatus = ResponseStatus.Error,
                IsSuccessStatusCode = false,
                Content = "Error"
            };

            MockClient
                .Setup(c => c.ExecuteAsync(
                    It.IsAny<RestRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResponse);

            // ACT
            var result = await Controller.ActualizarMPagoPredeterminado(TestMPagoId, TestUsuarioId);

            // ASSERT
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
            Assert.Equal("Error al completar la publicación de la actividad del usuario", obj.Value);
        }
        #endregion


        #region ActualizarMPagoPredeterminado_LanzaExcepcion_Retorna500InternalServerError()
        [Fact]
        public async Task ActualizarMPagoPredeterminado_LanzaExcepcion_Retorna500InternalServerError()
        {
            // ARRANGE
            var expectedException = new Exception("Error inesperado");

            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<MPagoPredeterminadoCommand>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // ACT
            var result = await Controller.ActualizarMPagoPredeterminado(TestMPagoId, TestUsuarioId);

            // ASSERT
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
            Assert.Equal(expectedException, obj.Value);

            MockMediator.Verify(m => m.Send(
                    It.IsAny<GetMPagoPorIdQuery>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            MockClient.Verify(c => c.ExecuteAsync(
                    It.IsAny<RestRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion
    }
}
