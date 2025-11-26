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
    public class PagosController_GetTodosMPago_Tests
    {
        private readonly Mock<IMediator> MockMediator;
        private readonly Mock<IRestClient> MockClient;
        private readonly Mock<ILog> MockLogger;
        private readonly PagosController Controller;

        // --- DATOS ---
        private readonly List<MPagoDTO> ListaMPagos;

        public PagosController_GetTodosMPago_Tests()
        {
            MockMediator = new Mock<IMediator>();
            MockClient = new Mock<IRestClient>();
            MockLogger = new Mock<ILog>();

            Controller = new PagosController(
                MockMediator.Object,
                MockClient.Object,
                MockLogger.Object
            );

            ListaMPagos = new List<MPagoDTO>
            {
                new MPagoDTO
                {
                    IdMPago         = "mpago_1",
                    IdUsuario       = "usr_1",
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
                    IdUsuario       = "usr_2",
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


        #region GetTodosMPago_ExistenRegistros_Retorna200Ok()
        [Fact]
        public async Task GetTodosMPago_ExistenRegistros_Retorna200Ok()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<GetTodosMPagoQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(ListaMPagos);

            // ACT
            var result = await Controller.GetTodosMPago();

            // ASSERT
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);

            var body = Assert.IsType<List<MPagoDTO>>(ok.Value);
            Assert.Equal(2, body.Count);
        }
        #endregion


        #region GetTodosMPago_SinRegistros_Retorna404NotFound()
        [Fact]
        public async Task GetTodosMPago_SinRegistros_Retorna404NotFound()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<GetTodosMPagoQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<MPagoDTO>?)null);

            // ACT
            var result = await Controller.GetTodosMPago();

            // ASSERT
            var nf = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, nf.StatusCode);
            Assert.Equal("No se encontró ningun MPago", nf.Value);
        }
        #endregion



        #region GetTodosMPago_LanzaExcepcion_Retorna500InternalServerError()
        [Fact]
        public async Task GetTodosMPago_LanzaExcepcion_Retorna500InternalServerError()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<GetTodosMPagoQuery>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Error inesperado"));

            // ACT
            var result = await Controller.GetTodosMPago();

            // ASSERT
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
            Assert.NotNull(obj.Value); // normalmente el mensaje de error
        }
        #endregion

        #region GetTodosMPago_DebeInvocarMediatorConQueryCorrecta()
        [Fact]
        public async Task GetTodosMPago_DebeInvocarMediatorConQueryCorrecta()
        {
            // ARRANGE
            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<GetTodosMPagoQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MPagoDTO>());

            // ACT
            await Controller.GetTodosMPago();

            // ASSERT
            MockMediator.Verify(m => m.Send(
                    It.Is<GetTodosMPagoQuery>(q => q != null),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion
    }
}
