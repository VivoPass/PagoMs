using log4net;
using MongoDB.Bson;
using MongoDB.Driver;
using Pagos.Domain.Aggregates;
using Pagos.Domain.Exceptions;
using Pagos.Domain.Interfaces;
using Pagos.Domain.ValueObjects;
using Pagos.Infrastructure.Configurations;
using Pagos.Infrastructure.Interfaces;


namespace Pagos.Infrastructure.Persistences.Repositories.MongoDB
{
    /// <summary>
    /// Repositorio para la gestión de la persistencia del agregado <see cref="Pago"/> en MongoDB.
    /// Implementa <see cref="IPagoRepository"/>.
    /// </summary>
    public class PagoRepository : IPagoRepository
    {
        private readonly IMongoCollection<BsonDocument> PagoColexion;
        private readonly IPagoFactory PagoFactory;
        private readonly IAuditoriaRepository AuditoriaRepository;
        private readonly ILog Log;

        public PagoRepository(PagoDbConfig mongoConfig, IPagoFactory pagoFactory, IAuditoriaRepository auditoriaRepository, ILog log)
        {
            PagoColexion = mongoConfig.db.GetCollection<BsonDocument>("pagos");
            PagoFactory = pagoFactory ?? throw new PagoFactoryNullException();
            AuditoriaRepository = auditoriaRepository ?? throw new AuditoriaRepositoryNullException();
            Log = log ?? throw new LogNullException();
        }

        #region AgregarPago(Pago pago)
        /// <summary>
        /// Agrega un nuevo registro de pago a la base de datos.
        /// </summary>
        /// <param name="pago">El agregado <see cref="Pago"/> a persistir.</param>
        /// <returns>Tarea asincrónica que representa la operación de escritura.</returns>
        /// <exception cref="ErrorConexionBd">Lanzada si falla la conexión con MongoDB.</exception>
        /// <exception cref="PagoRepositoryException">Lanzada si ocurre otro error inesperado.</exception>
        async public Task AgregarPago(Pago pago)
        {
            var idPago = pago.IdPago.Valor;
            var idUsuario = pago.IdUsuario.Valor;
            Log.Debug($"[CREATE] Intentando agregar Pago ID: {idPago} para Usuario ID: {idUsuario}.");
            try
            {
                var pagoInsert = new BsonDocument
                {
                    { "_id", idPago },
                    { "idUsuario", idUsuario},
                    { "idMPago", pago.IdMPago.Valor },
                    { "idReserva", pago.IdReserva.Valor },
                    { "idEvento", pago.IdEvento.Valor },
                    { "monto", pago.Monto.Valor },
                    { "fechaPago", pago.FechaPago.Valor },
                    { "idExternalPago", pago.IdExternalPago?.Valor ?? ""}
                };
                await PagoColexion.InsertOneAsync(pagoInsert);
                Log.Info($"[CREATE SUCCESS] Pago ID {idPago} insertado en MongoDB. Monto: {pago.Monto.Valor}.");

                await AuditoriaRepository.InsertarAuditoriaPago(pago.IdUsuario.Valor, "INFO", "PAGO_REGISTRADO", pago.IdPago.Valor, 
                    pago.IdMPago.Valor, pago.Monto.Valor, pago.IdReserva.Valor, 
                    $"Se registró el pago '{pago.IdPago.Valor}' del usuario '{pago.IdUsuario.Valor}' con respecto a la reserva " +
                    $"'{pago.IdReserva.Valor}' por el monto de '{pago.Monto.Valor}'.");
                Log.Debug($"[AUDITORIA SUCCESS] Auditoría registrada para Pago ID {idPago}.");
            }
            catch (MongoConnectionException ex)
            {
                Log.Error($"[CREATE ERROR] Error de conexión a DB al intentar agregar Pago ID {idPago}.", ex);
                throw new ErrorConexionBd(ex);
            }
            catch (Exception ex)
            {
                Log.Error($"[CREATE ERROR] Error inesperado al intentar agregar Pago ID {idPago}.", ex);
                throw new PagoRepositoryException(ex);
            }
        }
        #endregion

        #region ObtenerPagoPorIdPago(string idPago)
        /// <summary>
        /// Busca y obtiene un pago específico por su ID único.
        /// </summary>
        /// <param name="idPago">ID del pago a buscar.</param>
        /// <returns>El agregado <see cref="Pago"/> o null si no se encuentra (aunque relanza excepción de dominio).</returns>
        /// <exception cref="PagoNullRepositoryException">Lanzada si el pago no existe.</exception>
        /// <exception cref="ErrorConexionBd">Lanzada si falla la conexión con MongoDB.</exception>
        /// <exception cref="PagoRepositoryException">Lanzada si ocurre otro error inesperado.</exception>
        async public Task<Pago?> ObtenerPagoPorIdPago(string idPago)
        {
            Log.Debug($"[READ] Buscando Pago por ID: {idPago}.");
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", idPago);
                var pagoDoc = await PagoColexion.Find(filter).FirstOrDefaultAsync();

                if (pagoDoc == null)
                {
                    Log.Warn($"[READ WARN] Pago con ID {idPago} no encontrado en la base de datos.");
                    throw new PagoNullRepositoryException();
                }

                var IdPago = new VOIdPago(idPago);
                var IdMPago = new VOIdMPago(pagoDoc["idMPago"].AsString);
                var IdUsuario = new VOIdUsuario(pagoDoc["idUsuario"].AsString);
                var FechaPago = new VOFechaPago(pagoDoc["fechaPago"].ToLocalTime());
                var Monto = new VOMonto(pagoDoc["monto"].AsDecimal);
                var IdReserva = new VOIdReserva(pagoDoc["idReserva"].AsString);
                var IdEvento = new VOIdEvento(pagoDoc["idEvento"].AsString);
                var IdExternalPago = new VOIdExternalPago(pagoDoc["idExternalPago"].AsString);

                var pago = PagoFactory.Load(IdPago, IdMPago, IdUsuario, FechaPago, Monto, IdReserva, IdEvento, IdExternalPago);
                Log.Debug($"[READ SUCCESS] Pago {idPago} encontrado y mapeado.");

                return pago;
            }
            catch (PagoNullRepositoryException)
            {
                throw;
            }
            catch (MongoConnectionException ex)
            {
                Log.Error($"[READ ERROR] Error de conexión a DB al buscar Pago ID {idPago}.", ex);
                throw new ErrorConexionBd(ex);
            }
            catch (Exception ex)
            {
                Log.Error($"[READ ERROR] Error inesperado al buscar Pago ID {idPago}.", ex);
                throw new PagoRepositoryException(ex);
            }
        }
        #endregion

        #region ActualizarIdPagoExterno(string idPago, string idExternalPago)
        /// <summary>
        /// Actualiza el identificador externo de Stripe en un documento de pago existente.
        /// </summary>
        /// <param name="idPago">ID del pago que se desea actualizar.</param>
        /// <param name="idExternalPago">Nuevo ID externo a registrar.</param>
        /// <returns>Tarea asincrónica que representa la operación de actualización.</returns>
        /// <exception cref="ErrorConexionBd">Lanzada si falla la conexión con MongoDB.</exception>
        /// <exception cref="PagoRepositoryException">Lanzada si ocurre otro error inesperado.</exception>
        async public Task ActualizarIdPagoExterno(string idPago, string idExternalPago)
        {
            Log.Debug($"[UPDATE] Intentando actualizar idExternalPago para Pago ID: {idPago}. Nuevo ID externo: {idExternalPago}.");
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", idPago);
                var update = Builders<BsonDocument>.Update.Set("idExternalPago", idExternalPago);
                var result = await PagoColexion.UpdateOneAsync(filter, update);

                if (result.ModifiedCount == 0)
                {
                    Log.Warn($"[UPDATE WARN] No se encontró el Pago ID {idPago} para actualizar o el valor ya era '{idExternalPago}'. Documentos modificados: {result.ModifiedCount}.");
                }
                else
                {
                    Log.Info($"[UPDATE SUCCESS] idExternalPago actualizado para Pago ID {idPago}. Nuevo valor: {idExternalPago}.");
                }
            }
            catch (MongoConnectionException ex)
            {
                Log.Error($"[UPDATE ERROR] Error de conexión a DB al actualizar idExternalPago para Pago ID {idPago}.", ex);
                throw new ErrorConexionBd(ex);
            }
            catch (Exception ex)
            {
                Log.Error($"[UPDATE ERROR] Error inesperado al actualizar idExternalPago para Pago ID {idPago}.", ex);
                throw new PagoRepositoryException(ex);
            }
        }
        #endregion

        #region ObtenerPagosPorIdUsuario(string idUsuario)
        /// <summary>
        /// Obtiene una lista de todos los pagos realizados por un usuario específico.
        /// </summary>
        /// <param name="idUsuario">ID del usuario (Postor) cuyos pagos se desean obtener.</param>
        /// <returns>Lista de agregados <see cref="Pago"/>.</returns>
        /// <exception cref="PagoRepositoryException">Lanzada si ocurre un error inesperado durante la lectura.</exception>
        public async Task<List<Pago>> ObtenerPagosPorIdUsuario(string idUsuario)
        {
            Log.Debug($"[READ] Buscando todos los Pagos para el Usuario ID: {idUsuario}.");
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("idUsuario", idUsuario);
                var documentos = await PagoColexion.Find(filter).ToListAsync();
                var Pagos = new List<Pago>();

                foreach (var documento in documentos)
                {
                    var IdPago = new VOIdPago(documento["_id"].AsString);
                    var IdMPago = new VOIdMPago(documento["idMPago"].AsString);
                    var IdUsuario = new VOIdUsuario(idUsuario);
                    var FechaPago = new VOFechaPago(documento["fechaPago"].ToLocalTime());
                    var Monto = new VOMonto(documento["monto"].AsDecimal);
                    var IdReserva = new VOIdReserva(documento["idReserva"].AsString);
                    var IdEvento = new VOIdEvento(documento["idEvento"].AsString);
                    var IdExternalPago = new VOIdExternalPago(documento["idExternalPago"].AsString);

                    var pago = PagoFactory.Load(IdPago, IdMPago, IdUsuario, FechaPago, Monto, IdReserva, IdEvento, IdExternalPago);

                    Pagos.Add(pago);
                }
                Log.Info($"[READ SUCCESS] Se encontraron {Pagos.Count} Pagos para el Usuario ID {idUsuario}.");
                return Pagos;
            }
            catch (Exception ex)
            {
                Log.Error($"[READ ERROR] Error al buscar Pagos por Usuario ID {idUsuario}.", ex);
                throw new PagoRepositoryException(ex);
            }
        }
        #endregion

        #region ObtenerPagosPorIdEvento(string idEvento)
        /// <summary>
        /// Obtiene una lista de todos los pagos asociados a un evento específico.
        /// </summary>
        /// <param name="idEvento">ID del evento cuyos pagos se desean obtener.</param>
        /// <returns>Lista de agregados <see cref="Pago"/>.</returns>
        /// <exception cref="PagoRepositoryException">Lanzada si ocurre un error inesperado durante la lectura.</exception>
        public async Task<List<Pago>> ObtenerPagosPorIdEvento(string idEvento)
        {
            Log.Debug($"[READ] Buscando todos los Pagos asociados al Evento ID: {idEvento}.");
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("idEvento", idEvento);
                var documentos = await PagoColexion.Find(filter).ToListAsync();
                var Pagos = new List<Pago>();

                foreach (var documento in documentos)
                {
                    var IdPago = new VOIdPago(documento["_id"].AsString);
                    var IdMPago = new VOIdMPago(documento["idMPago"].AsString);
                    var IdUsuario = new VOIdUsuario(documento["idUsuario"].AsString);
                    var FechaPago = new VOFechaPago(documento["fechaPago"].ToLocalTime());
                    var Monto = new VOMonto(documento["monto"].AsDecimal);
                    var IdReserva = new VOIdReserva(documento["idReserva"].AsString);
                    var IdEvento = new VOIdEvento(idEvento);
                    var IdExternalPago = new VOIdExternalPago(documento["idExternalPago"].AsString);

                    var pago = PagoFactory.Load(IdPago, IdMPago, IdUsuario, FechaPago, Monto, IdReserva, IdEvento, IdExternalPago);

                    Pagos.Add(pago);
                }
                Log.Info($"[READ SUCCESS] Se encontraron {Pagos.Count} Pagos para el Evento ID {idEvento}.");
                return Pagos;
            }
            catch (Exception ex)
            {
                Log.Error($"[READ ERROR] Error al buscar Pagos por Evento ID {idEvento}.", ex);
                throw new PagoRepositoryException(ex);
            }
        }
        #endregion
    }
}
