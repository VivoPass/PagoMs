using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pagos.Application.Commands;
using Pagos.Application.DTOs;
using Pagos.Domain.Exceptions;
using Pagos.Infrastructure.Queries;

namespace Pagos.API.Controllers
{
    [ApiController]
    [Route("/api/Pagos")]
    public class PagosController : ControllerBase
    {
        private readonly IMediator Mediator;
        private readonly IPublishEndpoint PublishEndpoint;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="MPagoController"/> con las dependencias necesarias.
        /// </summary>
        /// <param name="mediator">
        /// Servicio de mediación utilizado para enviar comandos y consultas.
        /// </param>
        /// <param name="publishEndpoint">
        /// Endpoint para publicar eventos en el bus de mensajes.
        /// </param>
        /// <exception cref="MediatorNullException">
        /// Se lanza si <paramref name="mediator"/> es <c>null</c>.
        /// </exception>
        /// <exception cref="PublishEndpointNullException">
        /// Se lanza si <paramref name="publishEndpoint"/> es <c>null</c>.
        /// </exception>
        public PagosController(IMediator mediator, IPublishEndpoint publishEndpoint)
        {
            Mediator = mediator ?? throw new MediatorNullException();
            PublishEndpoint = publishEndpoint ?? throw new PublishEndpointNullException();
        }

        #region AgregarMPago([FromBody] AgregarMPagoStripeDto mpago)
        /// <summary>
        /// Agrega un nuevo método de pago usando Stripe.
        /// </summary>
        /// <param name="mpago">
        /// DTO <see cref="AgregarMPagoStripeDto"/> con la información requerida por Stripe
        /// para asociar un nuevo método de pago al postor.
        /// </param>
        /// <returns>
        /// <see cref="CreatedAtActionResult"/> (201) con el ID del nuevo método de pago si la operación es exitosa;
        /// <see cref="BadRequestObjectResult"/> (400) si no se pudo crear el recurso;
        /// <see cref="ObjectResult"/> (500) si ocurre un error interno.
        /// </returns>
        [HttpPost("agregarMPago")]
        public async Task<IActionResult> AgregarMPago([FromBody] AgregarMPagoStripeDTO mpago)
        {
            try
            {
                var IdMPago = await Mediator.Send(new AgregarMPagoCommand(mpago));
                if (IdMPago == null)
                {
                    return BadRequest("No se pudo agregar el mpago.");
                }
                return CreatedAtAction(nameof(AgregarMPago), new { id = IdMPago }, new
                {
                    id = IdMPago
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }
        #endregion

        #region EliminarMPago([FromQuery] string idMPago)
        /// <summary>
        /// Elimina un medio de pago (MPago) identificado por su ID.
        /// </summary>
        /// <param name="idMPago">
        /// Identificador único del medio de pago que se desea eliminar.
        /// </param>
        /// <returns>
        /// <see cref="OkObjectResult"/> (200) con mensaje de éxito si la eliminación fue exitosa;  
        /// <see cref="NotFoundObjectResult"/> (404) si no se pudo eliminar porque no se encontró el MPago;  
        /// <see cref="ObjectResult"/> (500) si ocurre un error interno en el servidor.
        /// </returns>
        [HttpDelete("eliminarMPago")]
        public async Task<IActionResult> EliminarMPago([FromQuery] string idMPago)
        {
            try
            {
                var result = await Mediator.Send(new EliminarMPagoCommand(idMPago));
                if (!result)
                {
                    return NotFound("El MPago no pudo ser eliminado.");
                }
                return Ok("MPago eliminado exitosamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }
        #endregion

        #region GetMPagoPorId([FromQuery] string idMPago)
        /// <summary>
        /// Recupera un medio de pago (MPago) por su identificador único.
        /// </summary>
        /// <param name="idMPago">Identificador del medio de pago a consultar.</param>
        /// <returns>
        /// <see cref="OkObjectResult"/> (200) con el documento MPago si existe;
        /// <see cref="NotFoundObjectResult"/> (404) si no se encuentra un MPago con ese ID;
        /// <see cref="ObjectResult"/> (500) si ocurre un error interno en el servidor.
        /// </returns>
        [HttpGet("getMPagoPorIdMPago")]
        public async Task<IActionResult> GetMPagoPorId([FromQuery] string idMPago)
        {
            try
            {
                var MPago = await Mediator.Send(new GetMPagoPorIdQuery(idMPago));

                if (MPago == null)
                {
                    return NotFound($"No se encontró un MPago con el id {idMPago}");
                }

                return Ok(MPago);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "InternalServerError",
                    Message = ex.Message,
                    Inner = ex.InnerException?.Message
                });
            }
        }
        #endregion

        #region GetMPagoPorIdUsuario([FromQuery] string idPostor)
        /// <summary>
        /// Recupera todos los medios de pago (MPago) asociados a un postor específico.
        /// </summary>
        /// <param name="idPostor">Identificador del postor cuyos MPagos se quieren listar.</param>
        /// <returns>
        /// <see cref="OkObjectResult"/> (200) con la lista de MPagos si existen;
        /// <see cref="NotFoundObjectResult"/> (404) si no se encuentra ningún MPago para ese postor;
        /// <see cref="ObjectResult"/> (500) si ocurre un error interno en el servidor.
        /// </returns>
        [HttpGet("getMPagoPorIdUsuario")]
        public async Task<IActionResult> GetMPagoPorIdUsuario([FromQuery] string idUsuario)
        {
            try
            {
                var MPago = await Mediator.Send(new GetMPagoPorIdUsuarioQuery(idUsuario));

                if (MPago == null)
                {
                    return NotFound($"No se encontró un MPago con el id del usuario {idUsuario}");
                }

                return Ok(MPago);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "InternalServerError",
                    Message = "Ocurrió un error inesperado al procesar la solicitud."
                });
            }
        }
        #endregion

        #region GetTodosMPago()
        /// <summary>
        /// Recupera todos los medios de pago (MPago) registrados.
        /// </summary>
        /// <returns>
        /// <see cref="OkObjectResult"/> (200) con la lista de todos los MPagos;
        /// <see cref="NotFoundObjectResult"/> (404) si no existe ningún MPago registrado;
        /// <see cref="ObjectResult"/> (500) si ocurre un error interno en el servidor.
        /// </returns>
        [HttpGet("getTodosMPago")]
        public async Task<IActionResult> GetTodosMPago()
        {
            try
            {
                var MPago = await Mediator.Send(new GetTodosMPagoQuery());

                if (MPago == null)
                {
                    return NotFound("No se encontró ningun MPago");
                }

                return Ok(MPago);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }
        #endregion

        #region ActualizarMPagoPredeterminado([FromQuery] string idMPago, [FromQuery] string idUsuario)
        /// <summary>
        /// Marca un medio de pago (MPago) como predeterminado para un postor específico.
        /// </summary>
        /// <param name="idMPago">
        /// Identificador del medio de pago que se desea establecer como predeterminado.
        /// </param>
        /// <param name="idPostor">
        /// Identificador del postor al que pertenece el medio de pago.
        /// </param>
        /// <returns>
        /// <see cref="OkObjectResult"/> (200) con mensaje de confirmación si la operación fue exitosa;
        /// <see cref="NotFoundObjectResult"/> (404) si no se pudo actualizar el MPago a predeterminado;
        /// <see cref="ObjectResult"/> (500) si ocurre un error interno en el servidor.
        /// </returns>
        [HttpPut("actualizarMPagoPredeterminado")]
        public async Task<IActionResult> ActualizarMPagoPredeterminado([FromQuery] string idMPago, [FromQuery] string idUsuario)
        {
            try
            {
                var result = await Mediator.Send(new MPagoPredeterminadoCommand(idMPago, idUsuario));
                if (!result)
                {
                    return NotFound("El MPago no pudo ser actualizado a predeterminado.");
                }
                return Ok("MPago actualizado a predeterminado exitosamente.");

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }
        #endregion
    }
}
