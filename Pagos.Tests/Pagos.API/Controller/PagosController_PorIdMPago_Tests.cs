using log4net;
using Moq;
using Pagos.API.Controllers;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pagos.Application.DTOs;
using Pagos.Infrastructure.Queries;

namespace Pagos.Tests.Pagos.API.Controller
{
    public class PagosController_PorIdMPago_Tests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<IRestClient> _restClient;
        private readonly Mock<ILog> _log;
        private readonly PagosController _controller;
        private readonly string _idMPago;

        public PagosController_PorIdMPago_Tests()
        {
            _mediator = new Mock<IMediator>();
            _restClient = new Mock<IRestClient>();
            _log = new Mock<ILog>();

            _controller = new PagosController(_mediator.Object, _restClient.Object, _log.Object);
            _idMPago = "mpago_123";

        }

        // 1) Flujo feliz → 200 OK con el MPago
        [Fact]
        public async Task GetMPagoPorId_DeberiaRetornarOk_CuandoExiste()
        {
            // Arrange
            var dto = new MPagoDTO
            {
                IdMPago = _idMPago,
                IdUsuario = "usr_test",
                IdMPagoStripe = "pm_xxx",
                IdClienteStripe = "cus_yyy",
                Marca = "Visa",
                MesExpiracion = 12,
                AnioExpiracion = 2030,
                Ultimos4 = "4242",
                FechaRegistro = DateTime.UtcNow,
                Predeterminado = true
            };

            _mediator
                .Setup(m => m.Send(
                    It.IsAny<IRequest<MPagoDTO>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            // Act
            var result = await _controller.GetMPagoPorId(_idMPago);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
            var body = Assert.IsType<MPagoDTO>(ok.Value);
            Assert.Equal(_idMPago, body.IdMPago);
        }

        [Fact]
        public async Task GetMPagoPorId_DeberiaRetornarNotFound_CuandoNoExiste()
        {
            // Arrange
            _mediator
                .Setup(m => m.Send(
                    It.IsAny<IRequest<MPagoDTO>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((MPagoDTO)null!);

            // Act
            var result = await _controller.GetMPagoPorId(_idMPago);

            // Assert
            var nf = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, nf.StatusCode);
            Assert.Equal($"No se encontró un MPago con el id {_idMPago}", nf.Value);
        }

        [Fact]
        public async Task GetMPagoPorId_DeberiaRetornar500_CuandoOcurreUnaExcepcion()
        {
            // Arrange
            _mediator
                .Setup(m => m.Send(
                    It.IsAny<IRequest<MPagoDTO>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Error inesperado"));

            // Act
            var result = await _controller.GetMPagoPorId(_idMPago);

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);

            // Opcional: validar estructura del body anónimo
            Assert.NotNull(obj.Value);
        }


        [Fact]
        public async Task GetMPagoPorId_DeberiaInvocarMediatorConLaQueryCorrecta()
        {
            // Arrange
            _mediator
                .Setup(m => m.Send(
                    It.IsAny<IRequest<MPagoDTO>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MPagoDTO { IdMPago = _idMPago });

            // Act
            await _controller.GetMPagoPorId(_idMPago);

            // Assert
            _mediator.Verify(m => m.Send(
                    It.Is<GetMPagoPorIdQuery>(q => q.IdMPago == _idMPago),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

    }
}
