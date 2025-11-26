using log4net;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pagos.Application.Commands;
using Pagos.Application.DTOs;
using Pagos.Domain.Exceptions;
using Pagos.Infrastructure.Queries;
using RestSharp;
using System.Net;

namespace Pagos.API.Controllers
{
    /// <summary>
    /// Controlador principal para la gestión de medios de pago y transacciones.
    /// </summary>
    [ApiController]
    [Route("/api/Pagos")]
    public class PagosController : ControllerBase
    {
        private readonly IMediator Mediator;
        private readonly IRestClient RestClient;
        private readonly ILog Log;
        public PagosController(IMediator mediator, IRestClient restClient, ILog log)
        {
            Mediator = mediator ?? throw new MediatorNullException();
            RestClient = restClient ?? throw new RestClientNullException();
            Log = log ?? throw new LogNullException();
        }
        
        #region AgregarMPago([FromBody] AgregarMPagoStripeDto mpago)
        /// <summary>
        /// Agrega un nuevo método de pago usando Stripe.
        /// </summary>
        /// <param name="mpago">
        /// DTO <see cref="AgregarMPagoStripeDTO"/> con la información requerida por Stripe
        /// para asociar un nuevo método de pago al usuario.
        /// </param>
        /// <returns>
        /// <see cref="CreatedAtActionResult"/> (201) con el ID del nuevo método de pago si la operación es exitosa;
        /// <see cref="BadRequestObjectResult"/> (400) si no se pudo crear el recurso;
        /// <see cref="ObjectResult"/> (500) si ocurre un error interno.
        /// </returns>
        [HttpPost("agregarMPago")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))] //201 OK
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))] // Error de comunicación externa o genérico
        public async Task<IActionResult> AgregarMPago([FromBody] AgregarMPagoStripeDTO mpago)
        {
            Log.Debug($"Inicio de AgregarMPago para el usuario: {mpago.IdUsuario}");
            try
            {
                var IdMPago = await Mediator.Send(new AgregarMPagoCommand(mpago));
                if (IdMPago == null)
                {
                    Log.Warn($"[400] No se pudo agregar el método de pago para el usuario {mpago.IdUsuario}. " +
                             $"Posiblemente fallo en Stripe o validación.");
                    return BadRequest("No se pudo agregar el mpago.");
                }

                //Conexion con el MS de Usuarios para la publicacion de la actividad
                var MPago = await Mediator.Send(new GetMPagoPorIdQuery(IdMPago));
                Log.Debug($"[MS-USUARIOS] Preparando publicación de actividad para usuario: {MPago.IdUsuario} (MPago: {MPago.IdMPago}).");
                var requestUsuario = new RestRequest(Environment.GetEnvironmentVariable("USUARIOS_MS_URL") +
                                                     $"/publishActivity", Method.Post);
                var activityBody = new
                    { idUsuario = MPago.IdUsuario, accion = $"Se registró el método de pago que tiene estos últimos cuatro dígitos: '{MPago.Ultimos4}'." };
                requestUsuario.AddJsonBody(activityBody);

                Log.Debug($"[MS-USUARIOS] Enviando POST a {requestUsuario}. Payload: {{ userId: {MPago.IdUsuario}, activityType: 'MPagoAgregado', mPagoId: {MPago.IdMPago} }}");
                var responseUsuario = await RestClient.ExecuteAsync(requestUsuario);
                if (!responseUsuario.IsSuccessful)
                {
                    Log.Error($"[500] ERROR DE COMUNICACIÓN EXTERNA [MS-USUARIOS]: El registro del MPago {MPago.IdMPago} fue exitoso, " +
                              $"pero falló la publicación de la actividad del usuario {MPago.IdUsuario}." +
                              $" StatusCode: {responseUsuario.StatusCode}, Content: {responseUsuario.Content}",
                        responseUsuario.ErrorException);
                    return StatusCode(500, "Error al completar la publicación de la actividad del usuario");
                }
                Log.Info($"[MS-USUARIOS] Actividad de usuario {MPago.IdUsuario} para Mpago {MPago.IdMPago} publicada exitosamente. " +
                         $"Status: {responseUsuario.StatusCode}.");

                Log.Info($"[201] MPago creado exitosamente. ID: {IdMPago}, Usuario: {mpago.IdUsuario}.");
                return CreatedAtAction(nameof(AgregarMPago), new { id = IdMPago }, new
                {
                    id = IdMPago
                });
            }
            catch (Exception ex)
            {
                Log.Error($"[500] Error al intentar agregar MPago para el usuario {mpago.IdUsuario}.", ex);
                return StatusCode(500, new
                {
                    Error = "InternalServerError",
                    Message = ex.Message,
                    Inner = ex.InnerException?.Message
                });
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))] // Error de comunicación externa o genérico
        public async Task<IActionResult> EliminarMPago([FromQuery] string idMPago)
        {
            var MPago = await Mediator.Send(new GetMPagoPorIdQuery(idMPago));
            Log.Debug($"Inicio de Eliminación de MPago: {idMPago}");
            try
            {
                var result = await Mediator.Send(new EliminarMPagoCommand(idMPago));
                if (!result)
                {
                    Log.Warn($"[404] Intento de eliminar MPago fallido. MPago con ID {idMPago} no encontrado o ya eliminado.");
                    return NotFound("El MPago no pudo ser eliminado.");
                }

                //Conexion con el MS de Usuarios para la publicacion de la actividad
                Log.Debug($"[MS-USUARIOS] Preparando publicación de actividad para usuario: {MPago.IdUsuario} (MPago: {MPago.IdMPago}).");
                var requestUsuario = new RestRequest(Environment.GetEnvironmentVariable("USUARIOS_MS_URL") +
                                                     $"/publishActivity", Method.Post);
                var activityBody = new
                    { idUsuario = MPago.IdUsuario, accion = $"Se eliminó el método de pago que tenía estos últimos cuatro dígitos: '{MPago.Ultimos4}'." };
                requestUsuario.AddJsonBody(activityBody);

                Log.Debug($"[MS-USUARIOS] Enviando POST a {requestUsuario}. Payload: {{ userId: {MPago.IdUsuario}, activityType: 'MPagoEliminado', mPagoId: {MPago.IdMPago} }}");
                var responseUsuario = await RestClient.ExecuteAsync(requestUsuario);
                if (!responseUsuario.IsSuccessful)
                {
                    Log.Error($"[500] ERROR DE COMUNICACIÓN EXTERNA [MS-USUARIOS]: La eliminación del MPago {MPago.IdMPago} fue exitosa, " +
                              $"pero falló la publicación de la actividad del usuario {MPago.IdUsuario}." +
                              $" StatusCode: {responseUsuario.StatusCode}, Content: {responseUsuario.Content}",
                        responseUsuario.ErrorException);
                    return StatusCode(500, "Error al completar la publicación de la actividad del usuario");
                }
                Log.Info($"[MS-USUARIOS] Actividad de usuario {MPago.IdUsuario} para Mpago {MPago.IdMPago} publicada exitosamente. " +
                         $"Status: {responseUsuario.StatusCode}.");

                Log.Info($"[200] MPago eliminado exitosamente. ID: {idMPago}.");
                return Ok("MPago eliminado exitosamente.");
            }
            catch (Exception ex)
            {
                Log.Error($"[500] Error al intentar eliminar MPago con ID {idMPago}.", ex);
                return StatusCode(500, new
                {
                    Error = "InternalServerError",
                    Message = ex.Message,
                    Inner = ex.InnerException?.Message
                });
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))] //retorna un MPagoDTO
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetMPagoPorId([FromQuery] string idMPago)
        {
            Log.Debug($"Inicio de consulta de MPago por ID: {idMPago}");
            try
            {
                var MPago = await Mediator.Send(new GetMPagoPorIdQuery(idMPago));

                if (MPago == null)
                {
                    Log.Warn($"[404] MPago con ID {idMPago} no encontrado.");
                    return NotFound($"No se encontró un MPago con el id {idMPago}");
                }
                Log.Debug($"[200] MPago encontrado: {idMPago}.");
                return Ok(MPago);
            }
            catch (Exception ex)
            {
                Log.Error($"[500] Error al intentar obtener MPago por ID {idMPago}.", ex);
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
        /// Recupera todos los medios de pago (MPago) asociados a un usuario específico.
        /// </summary>
        /// <param name="idUsuario">Identificador del usuario cuyos MPagos se quieren listar.</param>
        /// <returns>
        /// <see cref="OkObjectResult"/> (200) con la lista de MPagos si existen;
        /// <see cref="NotFoundObjectResult"/> (404) si no se encuentra ningún MPago para ese postor;
        /// <see cref="ObjectResult"/> (500) si ocurre un error interno en el servidor.
        /// </returns>
        [HttpGet("getMPagoPorIdUsuario")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<object>))] //retorna List<MPagoDTO>
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetMPagoPorIdUsuario([FromQuery] string idUsuario)
        {
            Log.Debug($"Inicio de consulta de MPagos por Usuario ID: {idUsuario}");
            try
            {
                var MPago = await Mediator.Send(new GetMPagoPorIdUsuarioQuery(idUsuario));

                if (MPago == null)
                {
                    Log.Warn($"[404] No se encontró ningún MPago asociado al usuario {idUsuario}.");
                    return NotFound($"No se encontró un MPago con el id del usuario {idUsuario}");
                }

                Log.Debug($"[200] MPagos encontrados para el usuario {idUsuario}.");
                return Ok(MPago);
            }
            catch (Exception ex)
            {
                Log.Error($"[500] Error al intentar obtener MPagos por Usuario ID {idUsuario}.", ex);
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
        /// Recupera todos los medios de pago (MPago) registrados. (Ruta administrativa/debugging).
        /// </summary>
        /// <returns>
        /// <see cref="OkObjectResult"/> (200) con la lista de todos los MPagos;
        /// <see cref="NotFoundObjectResult"/> (404) si no existe ningún MPago registrado;
        /// <see cref="ObjectResult"/> (500) si ocurre un error interno en el servidor.
        /// </returns>
        [HttpGet("getTodosMPago")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<object>))] //retorna List<MPagoDTO>
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Exception))]
        public async Task<IActionResult> GetTodosMPago()
        {
            Log.Debug("Inicio de consulta de TODOS los MPagos.");
            try
            {
                var MPago = await Mediator.Send(new GetTodosMPagoQuery());

                if (MPago == null)
                {
                    Log.Warn("[404] No se encontró ningún MPago registrado.");
                    return NotFound("No se encontró ningun MPago");
                }

                Log.Debug("[200] Consulta de todos los MPagos completada.");
                return Ok(MPago);
            }
            catch (Exception ex)
            {
                Log.Error("[500] Error al intentar obtener todos los MPagos.", ex);
                return StatusCode(500, ex);
            }
        }
        #endregion

        #region ActualizarMPagoPredeterminado([FromQuery] string idMPago, [FromQuery] string idUsuario)
        /// <summary>
        /// Marca un medio de pago (MPago) como predeterminado para un usuario específico.
        /// </summary>
        /// <param name="idMPago">
        /// Identificador del medio de pago que se desea establecer como predeterminado.
        /// </param>
        /// <param name="idUsuario">
        /// Identificador del usuario al que pertenece el medio de pago.
        /// </param>
        /// <returns>
        /// <see cref="OkObjectResult"/> (200) con mensaje de confirmación si la operación fue exitosa;
        /// <see cref="NotFoundObjectResult"/> (404) si no se pudo actualizar el MPago a predeterminado;
        /// <see cref="ObjectResult"/> (500) si ocurre un error interno en el servidor.
        /// </returns>
        [HttpPut("actualizarMPagoPredeterminado")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))] // Error de comunicación externa o genérico
        public async Task<IActionResult> ActualizarMPagoPredeterminado([FromQuery] string idMPago, [FromQuery] string idUsuario)
        {
            Log.Debug($"Inicio de ActualizarMPagoPredeterminado. MPago: {idMPago}, Usuario: {idUsuario}");
            try
            {
                var result = await Mediator.Send(new MPagoPredeterminadoCommand(idMPago, idUsuario));
                if (!result)
                {
                    Log.Warn($"[404] Fallo al actualizar MPago {idMPago} a predeterminado para usuario {idUsuario}. MPago o Usuario no encontrado.");
                    return NotFound("El MPago no pudo ser actualizado a predeterminado.");
                }

                //Conexion con el MS de Usuarios para la publicacion de la actividad
                var MPago = await Mediator.Send(new GetMPagoPorIdQuery(idMPago));
                Log.Debug($"[MS-USUARIOS] Preparando publicación de actividad para usuario: {MPago.IdUsuario} (MPago: {MPago.IdMPago}).");
                var requestUsuario = new RestRequest(Environment.GetEnvironmentVariable("USUARIOS_MS_URL") +
                                                     $"/publishActivity", Method.Post);
                var activityBody = new
                    { idUsuario = MPago.IdUsuario, accion = $"Se actualizó el método de pago que tiene estos últimos cuatro dígitos: '{MPago.Ultimos4}' a Predeterminado." };
                requestUsuario.AddJsonBody(activityBody);

                Log.Debug($"[MS-USUARIOS] Enviando POST a {requestUsuario}. Payload: {{ userId: {MPago.IdUsuario}, activityType: 'MPagoExitoso', mPagoId: {MPago.IdMPago} }}");
                var responseUsuario = await RestClient.ExecuteAsync(requestUsuario);
                if (!responseUsuario.IsSuccessful)
                {
                    Log.Error($"[500] ERROR DE COMUNICACIÓN EXTERNA [MS-USUARIOS]: La actualización del MPago {MPago.IdMPago} fue exitosa, " +
                              $"pero falló la publicación de la actividad del usuario {MPago.IdUsuario}." +
                              $" StatusCode: {responseUsuario.StatusCode}, Content: {responseUsuario.Content}",
                        responseUsuario.ErrorException);
                    return StatusCode(500, "Error al completar la publicación de la actividad del usuario");
                }
                Log.Info($"[MS-USUARIOS] Actividad de usuario {MPago.IdUsuario} para Mpago {MPago.IdMPago} publicada exitosamente. " +
                         $"Status: {responseUsuario.StatusCode}.");

                Log.Info($"[200] MPago {idMPago} actualizado a predeterminado para usuario {idUsuario} exitosamente.");
                return Ok("MPago actualizado a predeterminado exitosamente.");

            }
            catch (Exception ex)
            {
                Log.Error($"[500] Error al intentar actualizar MPago {idMPago} a predeterminado para usuario {idUsuario}.", ex);
                return StatusCode(500, ex);
            }
        }
        #endregion


        #region AgregarPago([FromBody] AgregarPagoDTO pago, string idMPago)
        /// <summary>
        /// Procesa una solicitud de pago y confirma la reserva asociada en el Microservicio de Reservas.
        /// </summary>
        /// <param name="pago">DTO con los detalles del pago (Monto, ID Reserva, etc.).</param>
        /// <param name="idMPago">ID del medio de pago a utilizar.</param>
        /// <returns>
        /// <see cref="CreatedAtActionResult"/> (201) si el pago y la confirmación de reserva son exitosos;
        /// <see cref="BadRequestObjectResult"/> (400) si el pago no se pudo crear;
        /// <see cref="ObjectResult"/> (500) si el pago es exitoso pero falla la confirmación de la reserva o si ocurre un error interno.
        /// </returns>
        [HttpPost("AgregarPago")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> AgregarPago([FromBody] AgregarPagoDTO pago, [FromQuery] string idMPago)
        {
            Log.Debug($"Inicio de AgregarPago. ID Reserva: {pago.IdReserva}, ID MPago: {idMPago}");
            try
            {
                var mPago = await Mediator.Send(new GetMPagoPorIdQuery(idMPago));
                var IdPago = await Mediator.Send(new AgregarPagoCommand(pago, mPago.IdClienteStripe, mPago.IdMPagoStripe));

                if (IdPago == null)
                {
                    Log.Warn($"[400] MPago no encontrado con ID {idMPago} al intentar agregar pago.");
                    return BadRequest("No se pudo crear el pago.");
                }
                Log.Info($"[201 Parte 1] Pago creado en el sistema. ID Pago: {IdPago}. Iniciando confirmación de reserva...");

                //Conexion con el MS de Reservas para el cambio de estado de la reserva a confirmada
                var requestReserva = new RestRequest(Environment.GetEnvironmentVariable("RESERVAS_MS_URL") + 
                    $"/{pago.IdReserva}/confirmar", Method.Post);
                var responseReserva = await RestClient.ExecuteAsync(requestReserva);
                if (!responseReserva.IsSuccessful)
                {
                    Log.Error($"[500] ERROR DE COMUNICACIÓN EXTERNA: Pago {IdPago} exitoso, pero el MS de Reservas devolvió error al confirmar" +
                              $" reserva {pago.IdReserva}. StatusCode: {responseReserva.StatusCode}, Content: {responseReserva.Content}", 
                        responseReserva.ErrorException);
                    return StatusCode(500, "Error al completar la reserva");
                }

                //Conexion con el MS de Usuarios para la publicacion de la actividad
                Log.Debug($"[MS-USUARIOS] Preparando publicación de actividad para usuario: {pago.IdUsuario} (Pago: {IdPago}).");
                var requestUsuario = new RestRequest(Environment.GetEnvironmentVariable("USUARIOS_MS_URL") +
                                                     $"/publishActivity", Method.Post);
                var activityBody = new
                { idUsuario = pago.IdUsuario, accion = $"Se realizó el pago de la reserva '{pago.IdReserva}' por el monto de: {pago.Monto}." };
                requestUsuario.AddJsonBody(activityBody);

                Log.Debug($"[MS-USUARIOS] Enviando POST a {requestUsuario}. Payload: {{ userId: {pago.IdUsuario}, activityType: 'PagoExitoso', reservationId: {pago.IdReserva} }}");
                var responseUsuario = await RestClient.ExecuteAsync(requestUsuario);
                if (!responseUsuario.IsSuccessful)
                {
                    Log.Error($"[500] ERROR DE COMUNICACIÓN EXTERNA [MS-USUARIOS]: El pago {IdPago} fue exitoso, " +
                              $"pero falló la publicación de la actividad del usuario {pago.IdUsuario}." +
                        $" StatusCode: {responseUsuario.StatusCode}, Content: {responseUsuario.Content}",
                        responseUsuario.ErrorException);
                    return StatusCode(500, "Error al completar la publicación de la actividad del usuario");
                }
                Log.Info($"[MS-USUARIOS] Actividad de usuario {pago.IdUsuario} para pago {IdPago} publicada exitosamente. " +
                         $"Status: {responseUsuario.StatusCode}.");

                Log.Info($"[201] Transacción de pago y confirmación de reserva {pago.IdReserva} completadas. ID Pago: {IdPago}.");
                return CreatedAtAction(nameof(AgregarPago), new { id = IdPago }, new { id = IdPago });
            }
            catch (Exception ex)
            {
                Log.Error($"[500] Error grave al procesar la solicitud de pago para Reserva {pago?.IdReserva}.", ex);
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region GetPagoPorId([FromQuery] string idPago)
        /// <summary>
        /// Obtiene los detalles de un pago específico por su ID.
        /// </summary>
        /// <param name="idPago">Identificador del pago a consultar.</param>
        /// <returns>
        /// <see cref="OkObjectResult"/> (200) con el documento Pago si existe;
        /// <see cref="NotFoundObjectResult"/> (404) si no se encuentra el pago;
        /// <see cref="ObjectResult"/> (500) si ocurre un error interno en el servidor.
        /// </returns>
        [HttpGet("GetPagoPorId")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))] //retorna un PagoDTO
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetPagoPorId([FromQuery] string idPago)
        {
            Log.Debug($"Inicio de consulta de Pago por ID: {idPago}");
            try
            {
                var Pago = await Mediator.Send(new GetPagoPorIdQuery(idPago));

                if (Pago == null)
                {
                    Log.Warn($"[404] Pago con ID {idPago} no encontrado.");
                    return NotFound($"No se encontró un pago con el id {idPago}");
                }

                Log.Debug($"[200] Pago encontrado: {idPago}.");
                return Ok(Pago);
            }
            catch (Exception ex)
            {
                Log.Error($"[500] Error al intentar obtener Pago por ID {idPago}.", ex);
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region GetPagoPorIdEvento([FromQuery] string idEvento)
        /// <summary>
        /// Obtiene todos los pagos asociados a un evento específico.
        /// </summary>
        /// <param name="idEvento">Identificador del evento a consultar.</param>
        /// <returns>
        /// <see cref="OkObjectResult"/> (200) con la lista de pagos si existen;
        /// <see cref="NotFoundObjectResult"/> (404) si no se encontraron pagos para el evento;
        /// <see cref="ObjectResult"/> (500) si ocurre un error interno en el servidor.
        /// </returns>
        [HttpGet("GetPagoPorIdEvento")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<object>))] //retorna List<PagoDTO>
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetPagoPorIdEvento([FromQuery] string idEvento)
        {
            Log.Debug($"Inicio de consulta de Pagos por ID de Evento: {idEvento}");
            try
            {
                var Pagos = await Mediator.Send(new GetPagosByIdEventoQuery(idEvento));

                if (Pagos == null)
                {
                    Log.Warn($"[404] No se encontraron pagos para el Evento con ID {idEvento}.");
                    return NotFound($"No se encontraron pagos con el id {idEvento}");
                }

                Log.Debug($"[200] Consulta de Pagos por Evento {idEvento} completada.");
                return Ok(Pagos);
            }
            catch (Exception ex)
            {
                Log.Error($"[500] Error al intentar obtener Pagos por ID de Evento {idEvento}.", ex);
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region GetPagosPorIdUsuario([FromQuery] string idUsuario)
        /// <summary>
        /// Obtiene todos los pagos realizados por un usuario específico.
        /// </summary>
        /// <param name="idUsuario">Identificador del usuario cuyos pagos se quieren listar.</param>
        /// <returns>
        /// <see cref="OkObjectResult"/> (200) con la lista de pagos del usuario (puede ser una lista vacía);
        /// <see cref="ObjectResult"/> (500) si ocurre un error interno en el servidor.
        /// </returns>
        [HttpGet("GetPagosPorIdUsuario")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<object>))] //retorna List<PagoDTO>
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetPagosPorIdUsuario([FromQuery] string idUsuario)
        {
            Log.Debug($"Inicio de consulta de Pagos por ID de Usuario: {idUsuario}");
            try
            {
                var pagos = await Mediator.Send(new GetPagosByIdUsuarioQuery(idUsuario));

                if (pagos == null)
                {
                    Log.Warn($"[404] Pagos del usuario ID {idUsuario} no encontrados.");
                    return NotFound($"No se encontraron pagos realizados por el usuario con id {idUsuario}");
                }

                Log.Debug($"[200] Consulta de Pagos por Usuario {idUsuario} completada. Registros encontrados: {pagos?.Count}.");
                return Ok(pagos);
            }
            catch (Exception ex)
            {
                Log.Error($"[500] Error al intentar obtener Pagos por ID de Usuario {idUsuario}.", ex);
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

    }
}
