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
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pagos.Tests.Pagos.API.Controller
{
    public class PagosController_GetMPagoPorIdUsuario_Tests
    {
        private readonly Mock<IMediator> MockMediator;
        private readonly Mock<IRestClient> MockClient;
        private readonly Mock<ILog> MockLogger;
        private readonly PagosController Controller;

        // --- DATOS ---
        private readonly string TestUsuarioId;
        private readonly List<MPagoDTO> ListaMPagos;

        public PagosController_GetMPagoPorIdUsuario_Tests()
        {
            MockMediator = new Mock<IMediator>();
            MockClient = new Mock<IRestClient>();
            MockLogger = new Mock<ILog>();

            Controller = new PagosController(
                MockMediator.Object,
                MockClient.Object,
                MockLogger.Object
            );

            TestUsuarioId = "usr_test_123";

            ListaMPagos = new List<MPagoDTO>
            {
                new MPagoDTO
                {
                    IdMPago         = "mpago_1",
                    IdUsuario       = TestUsuarioId,
                    IdMPagoStripe   = "pm_1",
                    IdClienteStripe = "cus_1",
                    Marca           = "Visa",
                    MesExpiracion   = 12,
                    AnioExpiracion  = 2030,
                    Ultimos4        = "4242",
                    FechaRegistro   = DateTime.UtcNow,
                    Predeterminado  = false
                },
                new MPagoDTO
                {
                    IdMPago         = "mpago_2",
                    IdUsuario       = TestUsuarioId,
                    IdMPagoStripe   = "pm_2",
                    IdClienteStripe = "cus_2",
                    Marca           = "Mastercard",
                    MesExpiracion   = 6,
                    AnioExpiracion  = 2031,
                    Ultimos4        = "1111",
                    FechaRegistro   = DateTime.UtcNow,
                    Predeterminado  = true
                }
            };
        }


        #region GetMPagoPorIdUsuario_ExistenRegistros_Retorna200Ok()
        [Fact]
        public async Task GetMPagoPorIdUsuario_ExistenRegistros_Retorna200Ok()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<GetMPagoPorIdUsuarioQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(ListaMPagos);

            // ACT
            var result = await Controller.GetMPagoPorIdUsuario(TestUsuarioId);

            // ASSERT
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);

            var body = Assert.IsType<List<MPagoDTO>>(ok.Value);
            Assert.Equal(2, body.Count);
        }
        #endregion


        #region GetMPagoPorIdUsuario_SinRegistros_Retorna404NotFound()
        [Fact]
        public async Task GetMPagoPorIdUsuario_SinRegistros_Retorna404NotFound()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<GetMPagoPorIdUsuarioQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<MPagoDTO>)null!);

            // ACT
            var result = await Controller.GetMPagoPorIdUsuario(TestUsuarioId);

            // ASSERT
            var nf = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, nf.StatusCode);
            Assert.Equal($"No se encontró un MPago con el id del usuario {TestUsuarioId}", nf.Value);
        }
        #endregion


        #region GetMPagoPorIdUsuario_LanzaExcepcion_Retorna500InternalServerError()
        [Fact]
        public async Task GetMPagoPorIdUsuario_LanzaExcepcion_Retorna500InternalServerError()
        {
            // ARRANGE
            var expectedException = new Exception("Error inesperado");

            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<GetMPagoPorIdUsuarioQuery>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // ACT
            var result = await Controller.GetMPagoPorIdUsuario(TestUsuarioId);

            // ASSERT
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
            Assert.NotNull(obj.Value); // body anónimo { Error, Message } en tu controller
        }
        #endregion


        #region GetMPagoPorIdUsuario_InvocaMediatorConQueryCorrecta()
        [Fact]
        public async Task GetMPagoPorIdUsuario_InvocaMediatorConQueryCorrecta()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<GetMPagoPorIdUsuarioQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MPagoDTO>());

            // ACT
            await Controller.GetMPagoPorIdUsuario(TestUsuarioId);

            // ASSERT
            MockMediator.Verify(m => m.Send(
                    It.Is<GetMPagoPorIdUsuarioQuery>(q => q.IdUsuario == TestUsuarioId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion
    }
}
