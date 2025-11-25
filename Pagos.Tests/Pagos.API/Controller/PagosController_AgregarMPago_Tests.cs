using log4net;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pagos.API.Controllers;
using Pagos.Application.Commands;
using Pagos.Application.DTOs;
using Pagos.Domain.Entities;
using Pagos.Infrastructure.Queries;
using RestSharp;
using System.Net;

namespace Pagos.Tests.Pagos.API.Controller
{
    public class PagosController_AgregarMPago_Tests
    {
        private readonly Mock<IMediator> MockMediator;
        private readonly Mock<IRestClient> MockClient;
        private readonly Mock<ILog> MockLogger;
        private readonly PagosController Controller;

        // --- DATOS ---
        private readonly AgregarMPagoStripeDTO ValidDto;
        private readonly string TestUserId = Guid.NewGuid().ToString();
        private readonly MPagoDTO ValidMPago;
        private readonly string TestMPagoId = "mpago_test_456";

        public PagosController_AgregarMPago_Tests()
        {
            MockMediator = new Mock<IMediator>();
            MockClient = new Mock<IRestClient>();
            MockLogger = new Mock<ILog>();

            Controller = new PagosController( MockMediator.Object, MockClient.Object, MockLogger.Object );

            // --- DATOS ---
            ValidDto = new AgregarMPagoStripeDTO
            {
                IdUsuario = TestUserId,
                IdMPagoStripe = "pm_1O3lR7I5xL6hV2e9oPqYzA8S",
                CorreoUsuario = "test@gmail.com"
            };

            ValidMPago = new MPagoDTO {
                IdMPago = TestMPagoId,
                IdUsuario = TestUserId,
                IdMPagoStripe = "pm_1O3lR7I5xL6hV2e9oPqYzA8S",
                IdClienteStripe = "cus_P67z9D8gH5bK3mS2pL1rJ0o",
                Marca = "Visa",
                MesExpiracion = 12,
                AnioExpiracion = 2028,
                Ultimos4 = "4242",
                FechaRegistro = new DateTime(2025, 11, 25, 10, 30, 0, DateTimeKind.Utc),
                Predeterminado = true
            };

        }

        #region AgregarMPago_CreacionExitosa_Retorna201Created()
        [Fact]
        public async Task AgregarMPago_CreacionExitosa_Retorna201Created()
        {
            // ARRANGE
            MockMediator.Setup(m => m.Send(It.IsAny<AgregarMPagoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestMPagoId);
            MockMediator.Setup(m => m.Send(It.IsAny<GetMPagoPorIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidMPago);
            // Simular la respuesta del MS de Usuarios
            var successfulResponse = new RestResponse
            {
                ResponseStatus = ResponseStatus.Completed,
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccessStatusCode = true
            };
            // Simular la comunicación con el MS de Usuarios
            MockClient.Setup(rc => rc.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(successfulResponse);

            // ACT
            var result = await Controller.AgregarMPago(ValidDto);

            // ASSERT
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
        }
        #endregion

        #region AgregarMPago_CreacionExitosa_RetornaIdMPago()
        [Fact]
        public async Task AgregarMPago_CreacionExitosa_RetornaIdMPago()
        {
            // ARRANGE
            MockMediator.Setup(m => m.Send(It.IsAny<AgregarMPagoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestMPagoId);
            MockMediator.Setup(m => m.Send(It.IsAny<GetMPagoPorIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidMPago);
            // Simular la respuesta del MS de Usuarios
            var successfulResponse = new RestResponse
            {
                ResponseStatus = ResponseStatus.Completed,
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccessStatusCode = true
            };
            // Simular la comunicación con el MS de Usuarios
            MockClient.Setup(rc => rc.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(successfulResponse);

            // ACT
            var result = await Controller.AgregarMPago(ValidDto);

            // ASSERT
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var idProperty = createdResult.Value.GetType().GetProperty("id");
            var actualId = idProperty.GetValue(createdResult.Value, null);
            Assert.Equal(TestMPagoId, actualId);
        }
        #endregion

        #region AgregarMPago_CreacionExitosa_LlamadaMSUsuarioExitosa()
        [Fact]
        public async Task AgregarMPago_CreacionExitosa_LlamadaMSUsuarioExitosa()
        {
            // ARRANGE
            MockMediator.Setup(m => m.Send(It.IsAny<AgregarMPagoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestMPagoId);
            MockMediator.Setup(m => m.Send(It.IsAny<GetMPagoPorIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidMPago);
            // Simular la respuesta del MS de Usuarios
            var successfulResponse = new RestResponse
            {
                ResponseStatus = ResponseStatus.Completed,
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccessStatusCode = true
            };
            // Simular la comunicación con el MS de Usuarios
            MockClient.Setup(rc => rc.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(successfulResponse);

            // ACT
            var result = await Controller.AgregarMPago(ValidDto);

            // ASSERT
            MockClient.Verify(rc => rc.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        #endregion


        #region AgregarMPago_FalloEnCreacionMPago_Retorna400BadRequest()
        [Fact]
        public async Task AgregarMPago_FalloEnCreacionMPago_Retorna400BadRequest()
        {
            // ARRANGE
            // Simular que el Command falla y devuelve null
            MockMediator.Setup(m => m.Send(It.IsAny<AgregarMPagoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string)null);

            // ACT
            var result = await Controller.AgregarMPago(ValidDto);

            // ASSERT
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("No se pudo agregar el mpago.", badRequestResult.Value);
        }
        #endregion

        #region AgregarMPago_FalloEnCreacionMPago_NoHaceLlamadas()
        [Fact]
        public async Task AgregarMPago_FalloEnCreacionMPago_NoHaceLlamadas()
        {
            // ARRANGE
            // Simular que el Command falla y devuelve null
            MockMediator.Setup(m => m.Send(It.IsAny<AgregarMPagoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string)null);

            // ACT
            var result = await Controller.AgregarMPago(ValidDto);

            // ASSERT
            MockMediator.Verify(m => m.Send(It.IsAny<GetMPagoPorIdQuery>(), It.IsAny<CancellationToken>()), Times.Never);
            MockClient.Verify(rc => rc.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        #endregion

        #region AgregarMPago_FalloPublicacionActividad_Retorna500ExternalServerError()
        [Fact]
        public async Task AgregarMPago_FalloPublicacionActividad_Retorna500ExternalServerError()
        {
            // ARRANGE
            MockMediator.Setup(m => m.Send(It.IsAny<AgregarMPagoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestMPagoId);
            MockMediator.Setup(m => m.Send(It.IsAny<GetMPagoPorIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidMPago);
            //Simular que la comunicación con el MS de Usuarios FALLA (IsSuccessful = false)
            var failedResponse = new RestResponse
            {
                ResponseStatus = ResponseStatus.Completed,
                StatusCode = System.Net.HttpStatusCode.InternalServerError,
                IsSuccessStatusCode = false
            };

            MockClient.Setup(rc => rc.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResponse);

            // ACT
            var result = await Controller.AgregarMPago(ValidDto);

            // ASSERT
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Error al completar la publicación de la actividad del usuario", statusCodeResult.Value);
        }
        #endregion

        #region AgregarMPago_LanzaExcepcion_Retorna500InternalServerError()
        [Fact]
        public async Task AgregarMPago_LanzaExcepcion_Retorna500InternalServerError()
        {
            // ARRANGE
            var expectedExceptionMessage = new Exception("Database connection lost.");
            MockMediator.Setup(m => m.Send(It.IsAny<AgregarMPagoCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedExceptionMessage);

            // ACT
            var result = await Controller.AgregarMPago(ValidDto);

            // ASSERT
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal(expectedExceptionMessage, statusCodeResult.Value);
        }
        #endregion

        #region AgregarMPago_LanzaExcepcion_NoLlamaMSUsuarios()
        [Fact]
        public async Task AgregarMPago_LanzaExcepcion_NoLlamaMSUsuarios()
        {
            // ARRANGE
            var expectedExceptionMessage = new Exception("Database connection lost.");
            MockMediator.Setup(m => m.Send(It.IsAny<AgregarMPagoCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedExceptionMessage);

            // ACT
            var result = await Controller.AgregarMPago(ValidDto);

            // ASSERT
            // Verificar que el RestClient NUNCA fue llamado (el error ocurrió antes)
            MockClient.Verify(rc => rc.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        #endregion
    }
}
