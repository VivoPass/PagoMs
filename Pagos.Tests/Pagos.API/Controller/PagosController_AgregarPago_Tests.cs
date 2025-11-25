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
using Pagos.Domain.ValueObjects;
using Pagos.Infrastructure.Queries;
using RestSharp;
using System.Net;

namespace Pagos.Tests.Pagos.API.Controller
{
    public class PagosController_AgregarPago_Tests
    {
        private readonly Mock<IMediator> MockMediator;
        private readonly Mock<IRestClient> MockClient;
        private readonly Mock<ILog> MockLogger;
        private readonly PagosController Controller;

        // --- DATOS ---
        private readonly AgregarPagoDTO ValidDto;
        private readonly string TestUserId = Guid.NewGuid().ToString();
        private readonly MPagoDTO ValidMPago;
        private readonly PagoDTO ValidPago;
        private readonly string TestMPagoId = "mpago_test_456";
        private readonly string TestPagoId = "pago_test_456";

        public PagosController_AgregarPago_Tests()
        {
            MockMediator = new Mock<IMediator>();
            MockClient = new Mock<IRestClient>();
            MockLogger = new Mock<ILog>();

            Controller = new PagosController(MockMediator.Object, MockClient.Object, MockLogger.Object);

            // --- DATOS ---
            ValidMPago = new MPagoDTO
            {
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

            ValidDto = new AgregarPagoDTO
            {
                IdUsuario = TestUserId,
                IdMPago = TestMPagoId,
                IdReserva = "reserva_XYZ456",
                IdEvento = "evento_UCAB_2025",
                FechaPago = DateTime.Now,
                Monto = 99.99m
            };

            ValidPago = new PagoDTO
            {
                IdPago = TestPagoId,
                IdMPago = TestMPagoId,
                IdUsuario = TestUserId,
                IdExternalPago = "stripe_pi_001A2B3C4D5E6F",
                IdReserva = "reserva_XYZ456",
                IdEvento = "evento_UCAB_2025",
                FechaPago = new DateTime(2025, 10, 25, 14, 30, 0, DateTimeKind.Utc),
                Monto = 99.99m
            };
        }

        #region AgregarPago_CreacionExitosa_Retorna201Created()
        [Fact]
        public async Task AgregarPago_CreacionExitosa_Retorna201Created()
        {
            // ARRANGE
            MockMediator.Setup(m => m.Send(It.IsAny<GetMPagoPorIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidMPago);
            MockMediator.Setup(m => m.Send(It.IsAny<AgregarPagoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestPagoId);
            // Simular la respuesta del MS de Reservas Y Usuarios
            var successfulResponseReservas = new RestResponse
            {
                ResponseStatus = ResponseStatus.Completed,
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccessStatusCode = true
            };
            var successfulResponseUsuarios = new RestResponse
            {
                ResponseStatus = ResponseStatus.Completed,
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccessStatusCode = true
            };
            // Simular la comunicación con el MS de Reservas Y Usuarios
            MockClient.SetupSequence(rc => rc.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(successfulResponseReservas)
                .ReturnsAsync(successfulResponseUsuarios);
            
            // ACT
            var result = await Controller.AgregarPago(ValidDto, TestMPagoId);

            // ASSERT
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
        }
        #endregion

        #region AgregarPago_CreacionExitosa_RetornaIdPago()
        [Fact]
        public async Task AgregarPago_CreacionExitosa_RetornaIdPago()
        {
            // ARRANGE
            MockMediator.Setup(m => m.Send(It.IsAny<GetMPagoPorIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidMPago);
            MockMediator.Setup(m => m.Send(It.IsAny<AgregarPagoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestPagoId);
            // Simular la respuesta del MS de Reservas Y Usuarios
            var successfulResponseReservas = new RestResponse
            {
                ResponseStatus = ResponseStatus.Completed,
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccessStatusCode = true
            };
            var successfulResponseUsuarios = new RestResponse
            {
                ResponseStatus = ResponseStatus.Completed,
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccessStatusCode = true
            };
            // Simular la comunicación con el MS de Reservas Y Usuarios
            MockClient.SetupSequence(rc => rc.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(successfulResponseReservas)
                .ReturnsAsync(successfulResponseUsuarios);

            // ACT
            var result = await Controller.AgregarPago(ValidDto, TestMPagoId);

            // ASSERT
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var idProperty = createdResult.Value.GetType().GetProperty("id");
            var actualId = idProperty.GetValue(createdResult.Value, null);
            Assert.Equal(TestPagoId, actualId);
        }
        #endregion

        #region AgregarPago_CreacionExitosa_LlamadaMSUsuarioYReservaExitosa()
        [Fact]
        public async Task AgregarPago_CreacionExitosa_LlamadaMSUsuarioYReservaExitosa()
        {
            // ARRANGE
            MockMediator.Setup(m => m.Send(It.IsAny<GetMPagoPorIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidMPago);
            MockMediator.Setup(m => m.Send(It.IsAny<AgregarPagoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestPagoId);
            // Simular la respuesta del MS de Reservas Y Usuarios
            var successfulResponseReservas = new RestResponse
            {
                ResponseStatus = ResponseStatus.Completed,
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccessStatusCode = true
            };
            var successfulResponseUsuarios = new RestResponse
            {
                ResponseStatus = ResponseStatus.Completed,
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccessStatusCode = true
            };
            // Simular la comunicación con el MS de Reservas Y Usuarios
            MockClient.SetupSequence(rc => rc.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(successfulResponseReservas)
                .ReturnsAsync(successfulResponseUsuarios);

            // ACT
            var result = await Controller.AgregarPago(ValidDto, TestMPagoId);

            // ASSERT
            MockClient.Verify(rc => rc.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
        #endregion


        #region AgregarPago_FalloEnCreacionPago_Retorna400BadRequest()
        [Fact]
        public async Task AgregarPago_FalloEnCreacionPago_Retorna400BadRequest()
        {
            // ARRANGE
            // Simular que el Command falla y devuelve null
            MockMediator.Setup(m => m.Send(It.IsAny<GetMPagoPorIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidMPago);
            MockMediator.Setup(m => m.Send(It.IsAny<AgregarPagoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string)null);

            // ACT
            var result = await Controller.AgregarPago(ValidDto, TestMPagoId);

            // ASSERT
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("No se pudo crear el pago.", badRequestResult.Value);
        }
        #endregion

        #region AgregarPago_FalloEnCreacionPago_NoHaceLlamadasClient()
        [Fact]
        public async Task AgregarPago_FalloEnCreacionPago_NoHaceLlamadasClient()
        {
            // ARRANGE
            // Simular que el Command falla y devuelve null
            MockMediator.Setup(m => m.Send(It.IsAny<GetMPagoPorIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidMPago);
            MockMediator.Setup(m => m.Send(It.IsAny<AgregarPagoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string)null);

            // ACT
            var result = await Controller.AgregarPago(ValidDto, TestMPagoId);

            // ASSERT
            MockClient.Verify(rc => rc.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        #endregion

        #region AgregarPago_FalloReserva_SeHizo1Llamada()
        [Fact]
        public async Task AgregarPago_FalloReserva_SeHizo1Llamada()
        {
            // ARRANGE
            MockMediator.Setup(m => m.Send(It.IsAny<GetMPagoPorIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidMPago);
            MockMediator.Setup(m => m.Send(It.IsAny<AgregarPagoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestPagoId);
            // Simular la respuesta del MS de Reservas
            var failedResponseReservas = new RestResponse
            {
                ResponseStatus = ResponseStatus.Completed,
                StatusCode = System.Net.HttpStatusCode.InternalServerError,
                IsSuccessStatusCode = false
            };
            // Simular la comunicación con el MS de Reservas
            MockClient.Setup(rc => rc.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResponseReservas);

            // ACT
            var result = await Controller.AgregarPago(ValidDto, TestMPagoId);

            // ASSERT
            MockClient.Verify(rc => rc.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        #endregion

        #region AgregarPago_FalloPublicacionActividad_Retorna500ExternalServerError()
        [Fact]
        public async Task AgregarPago_FalloPublicacionActividad_Retorna500ExternalServerError()
        {
            // ARRANGE
            MockMediator.Setup(m => m.Send(It.IsAny<GetMPagoPorIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidMPago);
            MockMediator.Setup(m => m.Send(It.IsAny<AgregarPagoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestPagoId);
            // Simular la respuesta del MS de Reservas Y Usuarios
            var successfulResponseReservas = new RestResponse
            {
                ResponseStatus = ResponseStatus.Completed,
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccessStatusCode = true
            };
            var failedResponseUsuarios = new RestResponse
            {
                ResponseStatus = ResponseStatus.Completed,
                StatusCode = System.Net.HttpStatusCode.InternalServerError,
                IsSuccessStatusCode = false
            };
            // Simular la comunicación con el MS de Reservas Y Usuarios
            MockClient.SetupSequence(rc => rc.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(successfulResponseReservas)
                .ReturnsAsync(failedResponseUsuarios);

            // ACT
            var result = await Controller.AgregarPago(ValidDto, TestMPagoId);

            // ASSERT
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Error al completar la publicación de la actividad del usuario", statusCodeResult.Value);
        }
        #endregion

        #region AgregarPago_FalloPublicacionActividad_SeHicieron2Llamadas()
        [Fact]
        public async Task AgregarPago_FalloPublicacionActividad_SeHicieron2Llamadas()
        {
            // ARRANGE
            MockMediator.Setup(m => m.Send(It.IsAny<GetMPagoPorIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidMPago);
            MockMediator.Setup(m => m.Send(It.IsAny<AgregarPagoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestPagoId);
            // Simular la respuesta del MS de Reservas Y Usuarios
            var successfulResponseReservas = new RestResponse
            {
                ResponseStatus = ResponseStatus.Completed,
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccessStatusCode = true
            };
            var failedResponseUsuarios = new RestResponse
            {
                ResponseStatus = ResponseStatus.Completed,
                StatusCode = System.Net.HttpStatusCode.InternalServerError,
                IsSuccessStatusCode = false
            };
            // Simular la comunicación con el MS de Reservas Y Usuarios
            MockClient.SetupSequence(rc => rc.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(successfulResponseReservas)
                .ReturnsAsync(failedResponseUsuarios);

            // ACT
            var result = await Controller.AgregarPago(ValidDto, TestMPagoId);

            // ASSERT
            MockClient.Verify(rc => rc.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
        #endregion

        #region AgregarPago_LanzaExcepcion_Retorna500InternalServerError()
        [Fact]
        public async Task AgregarPago_LanzaExcepcion_Retorna500InternalServerError()
        {
            // ARRANGE
            var expectedExceptionMessage = "Database connection lost.";
            MockMediator.Setup(m => m.Send(It.IsAny<GetMPagoPorIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidMPago);
            MockMediator.Setup(m => m.Send(It.IsAny<AgregarPagoCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(expectedExceptionMessage));

            // ACT
            var result = await Controller.AgregarPago(ValidDto, TestMPagoId);

            // ASSERT
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal(expectedExceptionMessage, statusCodeResult.Value);
        }
        #endregion

        #region AgregarPago_LanzaExcepcion_NoLlamaClient()
        [Fact]
        public async Task AgregarPago_LanzaExcepcion_NoLlamaClient()
        {
            // ARRANGE
            var expectedExceptionMessage = new Exception("Database connection lost.");
            MockMediator.Setup(m => m.Send(It.IsAny<GetMPagoPorIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidMPago);
            MockMediator.Setup(m => m.Send(It.IsAny<AgregarPagoCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedExceptionMessage);

            // ACT
            var result = await Controller.AgregarPago(ValidDto, TestMPagoId);

            // ASSERT
            // Verificar que el RestClient NUNCA fue llamado (el error ocurrió antes)
            MockClient.Verify(rc => rc.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        #endregion

        #region AgregarPago_MediatorThrowsException_Retorna500ExternalServerError()
        [Fact]
        public async Task AgregarPago_MediatorThrowsException_Retorna500ExternalServerError()
        {
            // ARRANGE
            var expectedException = new InvalidOperationException("Error de conexión a la base de datos de pagos.");
            MockMediator.Setup(m => m.Send(It.IsAny<GetMPagoPorIdQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // ACT
            var result = await Controller.AgregarPago(ValidDto, TestMPagoId);

            // ASSERT
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
            Assert.Equal(expectedException.Message, statusCodeResult.Value);
        }
        #endregion

        #region AgregarPago_MediatorThrowsException_NoLlamaClient()
        [Fact]
        public async Task AgregarPago_MediatorThrowsException_NoLlamaClient()
        {
            // ARRANGE
            var expectedException = new InvalidOperationException("Error de conexión a la base de datos de pagos.");
            MockMediator.Setup(m => m.Send(It.IsAny<GetMPagoPorIdQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // ACT
            var result = await Controller.AgregarPago(ValidDto, TestMPagoId);

            // ASSERT
            MockClient.Verify(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        #endregion
    }
}
