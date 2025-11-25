using log4net;
using MongoDB.Bson;
using MongoDB.Driver;
using Pagos.Domain.Exceptions;
using Pagos.Infrastructure.Configurations;
using Pagos.Infrastructure.Interfaces;

namespace Pagos.Infrastructure.Persistences.Repositories.MongoDB
{
    /// <summary>
    /// Repositorio encargado de registrar eventos de auditoría relacionados con Pagos y Métodos de Pago (MPago) en una colección de MongoDB.
    /// </summary>
    public class AuditoriaRepository: IAuditoriaRepository
    {
        private readonly IMongoCollection<BsonDocument> AuditoriaColexion;
        private readonly ILog Log;
        public AuditoriaRepository(AuditoriaDbConfig mongoConfig, ILog log)
        {
            AuditoriaColexion = mongoConfig.db.GetCollection<BsonDocument>("auditoriaPagos");
            Log = log ?? throw new LogNullException();
        }

        #region InsertarAuditoriaPago(string idUsuario, string idPago, string idMPago, decimal monto, string idReserva, string mensaje)
        /// <summary>
        /// Inserta un documento de auditoría detallado para transacciones de pago en la colección de MongoDB.
        /// </summary>
        /// <param name="idUsuario">ID del usuario que realiza la acción.</param>
        /// <param name="level">Nivel de la auditoría (e.g., "INFO", "ERROR", "DEBUG").</param>
        /// <param name="tipo">Tipo de evento de pago (e.g., "PAGO_EXITOSO", "REEMBOLSO", "ERROR_STRIPE").</param>
        /// <param name="idPago">ID del pago involucrado.</param>
        /// <param name="idMPago">ID del método de pago utilizado.</param>
        /// <param name="monto">Monto de la transacción.</param>
        /// <param name="idReserva">ID de la reserva asociada (si aplica).</param>
        /// <param name="mensaje">Mensaje o descripción detallada del evento.</param>
        /// <returns>Tarea asíncrona completada.</returns>
        /// <exception cref="AuditoriaRepositoryException">Se lanza si ocurre un error durante la inserción en MongoDB.</exception>
        public async Task InsertarAuditoriaPago
            (string idUsuario, string level, string tipo, string idPago, string idMPago, decimal monto, string idReserva, string mensaje)
        {
            var docId = Guid.NewGuid().ToString();
            Log.Debug($"[PAGO AUDIT] Iniciando inserción de auditoría de pago. Usuario: {idUsuario}, Pago: {idPago}," +
                      $" Reserva: {idReserva}, Level: {level}, Tipo: {tipo}. ID Doc: {docId}");

            try
            {
                var documento = new BsonDocument
                {
                    { "_id",  docId},
                    { "idUsuario", idUsuario},
                    { "idPago", idPago},
                    { "idMPago", idMPago},
                    { "idReserva", idReserva},
                    { "monto", monto},
                    { "level", level},
                    { "tipo", tipo},
                    { "mensaje", mensaje},
                    { "timestamp", DateTime.Now}
                };
                await AuditoriaColexion.InsertOneAsync(documento);
                Log.Info($"[PAGO AUDIT] Documento de auditoría de pago insertado exitosamente. ID de Documento: {docId}");
            }
            catch (Exception ex)
            {
                Log.Error($"[PAGO AUDIT] Error crítico al intentar insertar la auditoría de pago. Usuario: {idUsuario}, " +
                          $"Pago: {idPago}, Mensaje: {mensaje}.", ex);
                throw new AuditoriaRepositoryException(ex);
            }
        }
        #endregion

        #region InsertarAuditoriaMPago(string idUsuario, string idMPago, string mensaje)
        /// <summary>
        /// Inserta un documento de auditoría específico para eventos relacionados con Métodos de Pago (MPago) en la colección de MongoDB.
        /// </summary>
        /// <param name="idUsuario">ID del usuario asociado al método de pago.</param>
        /// <param name="level">Nivel de la auditoría (e.g., "INFO", "ERROR", "DEBUG").</param>
        /// <param name="tipo">Tipo de evento de MPago (e.g., "MPAGO_AGREGADO", "MPAGO_ELIMINADO", "ERROR_GUARDADO").</param>
        /// <param name="idMPago">ID del método de pago involucrado.</param>
        /// <param name="mensaje">Mensaje o descripción detallada del evento.</param>
        /// <returns>Tarea asíncrona completada.</returns>
        /// <exception cref="AuditoriaRepositoryException">Se lanza si ocurre un error durante la inserción en MongoDB.</exception>
        public async Task InsertarAuditoriaMPago(string idUsuario, string level, string tipo, string idMPago, string mensaje)
        {
            var docId = Guid.NewGuid().ToString();
            Log.Debug($"[MPAGO AUDIT] Iniciando inserción de auditoría de MPago. Usuario: {idUsuario}, MPago: {idMPago}, " +
                      $"Level: {level}, Tipo: {tipo}. ID Doc. Gen: {docId}");
            try
            {
                var documento = new BsonDocument
                {
                    { "_id",  docId},
                    { "idUsuario", idUsuario},
                    { "idMPago", idMPago},
                    { "level", level},
                    { "tipo", tipo},
                    { "mensaje", mensaje},
                    { "timestamp", DateTime.Now}
                };
                await AuditoriaColexion.InsertOneAsync(documento);
                Log.Info($"[MPAGO AUDIT] Documento de auditoría de método de pago insertado exitosamente. ID de Documento: {docId}");
            }
            catch (Exception ex)
            {
                Log.Error($"[MPAGO AUDIT] Error crítico al intentar insertar la auditoría de método de pago. " +
                          $"Usuario: {idUsuario}, MPago: {idMPago}, Mensaje: {mensaje}.", ex);
                throw new AuditoriaRepositoryException(ex);
            }
        }
        #endregion
    }
}
