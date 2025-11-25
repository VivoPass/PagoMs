using log4net;
using MongoDB.Bson;
using MongoDB.Driver;
using Pagos.Domain.Entities;
using Pagos.Domain.Exceptions;
using Pagos.Domain.Interfaces;
using Pagos.Domain.ValueObjects;
using Pagos.Infrastructure.Configurations;
using Pagos.Infrastructure.Interfaces;

namespace Pagos.Infrastructure.Persistences.Repositories.MongoDB
{
    /// <summary>
    /// Repositorio de métodos de pago (MPago) que maneja la persistencia en MongoDB.
    /// Implementa <see cref="IMPagoRepository"/> y utiliza BSON para la interacción de bajo nivel.
    /// </summary>
    public class MPagoRepository : IMPagoRepository
    {
        private readonly IMongoCollection<BsonDocument> MPagoColexion;
        private readonly ITarjetaCreditoFactory TarjetaCreditoFactory;
        private readonly IAuditoriaRepository AuditoriaRepository;
        private readonly ILog Log;

        public MPagoRepository(PagoDbConfig mongoConfig, ITarjetaCreditoFactory tarjetaCreditoFactory, IAuditoriaRepository auditoriaRepository, ILog log)
        {
            MPagoColexion = mongoConfig.db.GetCollection<BsonDocument>("mpagos");
            TarjetaCreditoFactory = tarjetaCreditoFactory ?? throw new TarjetaCreditoFactoryNullException();
            AuditoriaRepository = auditoriaRepository ?? throw new AuditoriaRepositoryNullException();
            Log = log ?? throw new LogNullException();
        }

        #region ObtenerMPagoPorId(string idMPago)
        /// <summary>
        /// Obtiene un método de pago por su ID único.
        /// </summary>
        /// <param name="idMPago">ID local del método de pago (MongoDB _id).</param>
        /// <returns>Objeto <see cref="TarjetaCredito"/> si se encuentra.</returns>
        /// <exception cref="MPagoNullRepositoryException">Lanzada si el documento no existe.</exception>
        /// <exception cref="MPagoRepositoryException">Lanzada en caso de error de conexión/lectura.</exception>
        public async Task<TarjetaCredito> ObtenerMPagoPorId(string idMPago)
        {
            Log.Debug($"[READ] Buscando MPago con ID: {idMPago}.");
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", idMPago);
                var documento = await MPagoColexion.Find(filter).FirstOrDefaultAsync();
                if (documento == null)
                {
                    Log.Warn($"[NOT FOUND] MPago con ID {idMPago} no encontrado.");
                    throw new MPagoNullRepositoryException();
                }
                
                var IdMPago = new VOIdMPago(idMPago);
                var IdUsuario = new VOIdUsuario(documento["idUsuario"].AsString);
                var IdMPagoStripe = new VOIdMPagoStripe(documento["idMPagoStripe"].AsString);
                var IdClienteStripe = new VOIdClienteStripe(documento["idClienteStripe"].AsString);
                var Marca = new VOMarca(documento["marca"].AsString);
                var MesExpiracion = new VOMesExpiracion(documento["mesExpiracion"].AsInt32);
                var AnioExpiracion = new VOAnioExpiracion(documento["anioExpiracion"].AsInt32);
                var Ultimos4 = new VOUltimos4(documento["ultimos4"].AsString);
                var FechaRegistro = new VOFechaRegistro(documento["fechaRegistro"].ToLocalTime());
                var Predeterminado = new VOPredeterminado(documento["predeterminado"].AsBoolean);

                var mpago = TarjetaCreditoFactory.Load(
                    IdMPago, IdUsuario, IdMPagoStripe, IdClienteStripe, Marca, MesExpiracion, AnioExpiracion, Ultimos4,
                    FechaRegistro, Predeterminado);

                Log.Debug($"[READ SUCCESS] MPago {idMPago} encontrado y mapeado.");
                return mpago;
            }
            catch (MPagoNullRepositoryException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Error($"[READ ERROR] Error al intentar obtener MPago con ID {idMPago}.", ex);
                throw new MPagoRepositoryException(ex);
            }
        }
        #endregion

        #region ObtenerMPagoPorIdUsuario(string idPostor)
        /// <summary>
        /// Obtiene todos los métodos de pago asociados a un usuario (Postor).
        /// </summary>
        /// <param name="idUsuario">ID del usuario (Postor).</param>
        /// <returns>Lista de objetos <see cref="TarjetaCredito"/>.</returns>
        /// <exception cref="MPagoRepositoryException">Lanzada en caso de error de conexión/lectura.</exception>
        public async Task<List<TarjetaCredito>> ObtenerMPagoPorIdUsuario(string idUsuario)
        {
            Log.Debug($"[READ] Buscando todos los MPagos para el usuario ID: {idUsuario}.");
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("idUsuario", idUsuario);
                var documentos = await MPagoColexion.Find(filter).ToListAsync();
                var mPagos = new List<TarjetaCredito>();

                foreach (var documento in documentos)
                {
                    var idMPago = new VOIdMPago(documento["_id"].AsString);
                    var IdUsuario = new VOIdUsuario(idUsuario);
                    var idMPagoStripe = new VOIdMPagoStripe(documento["idMPagoStripe"].AsString);
                    var idClienteStripe = new VOIdClienteStripe(documento["idClienteStripe"].AsString);
                    var marca = new VOMarca(documento["marca"].AsString);
                    var mesExpiracion = new VOMesExpiracion(documento["mesExpiracion"].AsInt32);
                    var anioExpiracion = new VOAnioExpiracion(documento["anioExpiracion"].AsInt32);
                    var ultimos4 = new VOUltimos4(documento["ultimos4"].AsString);
                    var fechaRegistro = new VOFechaRegistro(documento["fechaRegistro"].ToLocalTime());
                    var predeterminado = new VOPredeterminado(documento["predeterminado"].AsBoolean);

                    var mpago = TarjetaCreditoFactory.Load(
                        idMPago, IdUsuario, idMPagoStripe, idClienteStripe, marca, mesExpiracion, anioExpiracion, ultimos4,
                        fechaRegistro, predeterminado
                    );

                    mPagos.Add(mpago);
                }

                Log.Info($"[READ SUCCESS] Se encontraron {mPagos.Count} MPagos para el usuario {idUsuario}.");
                return mPagos;
            }
            catch (Exception ex)
            {
                Log.Error($"[READ ERROR] Error al intentar obtener MPagos por IdUsuario {idUsuario}.", ex);
                throw new MPagoRepositoryException(ex);
            }
        }
        #endregion

        #region AgregarMPago(TarjetaCredito mPago)
        /// <summary>
        /// Agrega un nuevo método de pago a la colección.
        /// </summary>
        /// <param name="mPago">El objeto <see cref="TarjetaCredito"/> a persistir.</param>
        /// <returns>El mismo objeto <see cref="TarjetaCredito"/> persistido.</returns>
        /// <exception cref="MPagoRepositoryException">Lanzada en caso de error de escritura.</exception>
        public async Task<TarjetaCredito> AgregarMPago(TarjetaCredito mPago)
        {
            var idMpago = mPago.IdMPago.Valor;
            var idUsuario = mPago.IdUsuario.Valor;
            Log.Debug($"[CREATE] Intentando agregar MPago con ID: {idMpago} para usuario {idUsuario}.");
            try
            {
                var documento = new BsonDocument
            {
                { "_id", idMpago },
                { "idUsuario", idUsuario },
                { "idMPagoStripe", mPago.IdMPagoStripe.Valor },
                { "idClienteStripe", mPago.IdClienteStripe.Valor },
                { "marca", mPago.Marca.Valor },
                { "mesExpiracion", mPago.MesExpiracion.Valor },
                { "anioExpiracion", mPago.AnioExpiracion.Valor },
                { "ultimos4", mPago.Ultimos4.Valor },
                { "fechaRegistro", mPago.FechaRegistro.Valor.ToLocalTime() },
                { "predeterminado", mPago.Predeterminado.Valor }
            };
                await MPagoColexion.InsertOneAsync(documento);
                Log.Info($"[CREATE SUCCESS] MPago {idMpago} agregado exitosamente. Insertando auditoría.");

                await AuditoriaRepository.InsertarAuditoriaMPago(mPago.IdUsuario.Valor, "INFO", "MPAGO_REGISTRADO", mPago.IdMPago.Valor,
                    $"Se registró el método de pago '{mPago.IdMPago.Valor}' del usuario '{mPago.IdUsuario.Valor}'.");

                return mPago;
            }
            catch (Exception ex)
            {
                Log.Error($"[CREATE ERROR] Error al agregar MPago ID {idMpago}.", ex);
                throw new MPagoRepositoryException(ex);
            }
        }
        #endregion

        #region ActualizarPredeterminadoTrueMPago(string idMPago)
        /// <summary>
        /// Establece el método de pago especificado como predeterminado (Predeterminado = true).
        /// </summary>
        /// <param name="idMPago">ID del método de pago a actualizar.</param>
        /// <exception cref="MPagoRepositoryException">Lanzada en caso de error de actualización.</exception>
        public async Task ActualizarPredeterminadoTrueMPago(string idMPago)
        {
            Log.Debug($"[UPDATE] Intentando establecer como PREDETERMINADO el MPago ID: {idMPago}.");
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", idMPago);
                var update = Builders<BsonDocument>.Update.Set("predeterminado", true);
                var result = await MPagoColexion.UpdateOneAsync(filter, update);

                if (result.ModifiedCount == 0)
                {
                    Log.Warn($"[UPDATE WARN] No se encontró el MPago con ID {idMPago} para establecer como predeterminado.");
                }

                var documento = await MPagoColexion.Find(filter).FirstOrDefaultAsync();
                if (documento != null)
                {
                    var idUsuario = documento["idUsuario"].AsString;
                    Log.Info($"[UPDATE SUCCESS] MPago {idMPago} establecido como predeterminado para el usuario {idUsuario}.");
                    await AuditoriaRepository.InsertarAuditoriaMPago(idUsuario, "INFO", "MPAGO_PREDETERMINADO_TRUE", idMPago,
                        $"Se actualizó el método de pago '{idMPago}' del usuario '{idUsuario}' para que sea Predeterminado.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[UPDATE ERROR] Error al establecer MPago ID {idMPago} como predeterminado.", ex);
                throw new MPagoRepositoryException(ex);
            }
        }
        #endregion

        #region ActualizarPredeterminadoFalseMPago(string idMPago)
        /// <summary>
        /// Desactiva el método de pago especificado como predeterminado (Predeterminado = false).
        /// </summary>
        /// <param name="idMPago">ID del método de pago a actualizar.</param>
        /// <exception cref="MPagoRepositoryException">Lanzada en caso de error de actualización.</exception>
        public async Task ActualizarPredeterminadoFalseMPago(string idMPago)
        {
            Log.Debug($"[UPDATE] Intentando desactivar como PREDETERMINADO el MPago ID: {idMPago}.");
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", idMPago);
                var update = Builders<BsonDocument>.Update.Set("predeterminado", false);
                var result = await MPagoColexion.UpdateOneAsync(filter, update);
                if (result.ModifiedCount == 0)
                {
                    Log.Warn($"[UPDATE WARN] No se encontró el MPago con ID {idMPago} para desactivar como predeterminado.");
                }

                var documento = await MPagoColexion.Find(filter).FirstOrDefaultAsync();
                if (documento != null)
                {
                    var idUsuario = documento["idUsuario"].AsString;
                    Log.Info($"[UPDATE SUCCESS] MPago {idMPago} desactivado como predeterminado para el usuario {idUsuario}.");
                    await AuditoriaRepository.InsertarAuditoriaMPago(idUsuario, "INFO", "MPAGO_PREDETERMINADO_FALSE", idMPago,
                        $"Se registró el método de pago '{idMPago}' del usuario '{idUsuario}' para que NO sea Predeterminado.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[UPDATE ERROR] Error al desactivar MPago ID {idMPago} como predeterminado.", ex);
                throw new MPagoRepositoryException(ex);
            }
        }
        #endregion

        #region EliminarMPago(string idMPago)
        /// <summary>
        /// Elimina un método de pago de la colección.
        /// </summary>
        /// <param name="idMPago">ID del método de pago a eliminar.</param>
        /// <exception cref="MPagoRepositoryException">Lanzada en caso de error de eliminación.</exception>
        public async Task EliminarMPago(string idMPago)
        {
            Log.Debug($"[DELETE] Intentando eliminar MPago con ID: {idMPago}.");
            try
            {
                var filterAuditoria = Builders<BsonDocument>.Filter.Eq("_id", idMPago);
                var documento = await MPagoColexion.Find(filterAuditoria).FirstOrDefaultAsync();
                var idUsuario = documento["idUsuario"].AsString;

                var filter = Builders<BsonDocument>.Filter.Eq("_id", idMPago);
                var result = await MPagoColexion.DeleteOneAsync(filter);

                if (result.DeletedCount == 0)
                {
                    Log.Warn($"[DELETE WARN] No se encontró el MPago con ID {idMPago} para eliminar.");
                }
                else
                {
                    Log.Info($"[DELETE SUCCESS] MPago {idMPago} eliminado exitosamente. Registrando auditoría.");
                    await AuditoriaRepository.InsertarAuditoriaMPago(idUsuario, "INFO", "MPAGO_ELIMINADO", idMPago,
                        $"Se eliminó el método de pago '{idMPago}' del usuario '{idUsuario}'.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[DELETE ERROR] Error al intentar eliminar MPago ID {idMPago}.", ex);
                throw new MPagoRepositoryException(ex);
            }
        }
        #endregion

        #region GetTodosMPago()
        /// <summary>
        /// Obtiene todos los métodos de pago en la base de datos (Nota: Solo para fines administrativos/desarrollo).
        /// </summary>
        /// <returns>Lista de todos los objetos <see cref="TarjetaCredito"/>.</returns>
        /// <exception cref="MPagoNullRepositoryException">Lanzada si no hay documentos en la colección.</exception>
        /// <exception cref="MPagoRepositoryException">Lanzada en caso de error de conexión o lectura.</exception>
        public async Task<List<TarjetaCredito>> GetTodosMPago()
        {
            Log.Debug("[READ] Buscando todos los MPagos en la colección.");
            try
            {
                var mpagos = await MPagoColexion.Find(_ => true).ToListAsync();
                if (mpagos == null || mpagos.Count == 0)
                {
                    Log.Warn("[READ WARN] No se encontraron MPagos en la colección (colección vacía).");
                    throw new MPagoNullRepositoryException();
                }
                var MPagos = new List<TarjetaCredito>();

                foreach (var documento in mpagos)
                {
                    var idMPago = new VOIdMPago(documento["_id"].AsString);
                    var IdUsuario = new VOIdUsuario(documento["idUsuario"].AsString);
                    var idMPagoStripe = new VOIdMPagoStripe(documento["idMPagoStripe"].AsString);
                    var idClienteStripe = new VOIdClienteStripe(documento["idClienteStripe"].AsString);
                    var marca = new VOMarca(documento["marca"].AsString);
                    var mesExpiracion = new VOMesExpiracion(documento["mesExpiracion"].AsInt32);
                    var anioExpiracion = new VOAnioExpiracion(documento["anioExpiracion"].AsInt32);
                    var ultimos4 = new VOUltimos4(documento["ultimos4"].AsString);
                    var fechaRegistro = new VOFechaRegistro(documento["fechaRegistro"].ToLocalTime());
                    var predeterminado = new VOPredeterminado(documento["predeterminado"].AsBoolean);

                    var mpago = TarjetaCreditoFactory.Load(
                        idMPago, IdUsuario, idMPagoStripe, idClienteStripe, marca, mesExpiracion, anioExpiracion, ultimos4,
                        fechaRegistro, predeterminado
                    );

                    MPagos.Add(mpago);
                }
                Log.Info($"[READ SUCCESS] Se obtuvieron {MPagos.Count} documentos de la colección 'mpagos'.");
                return MPagos;
            }
            catch (MongoConnectionException ex)
            {
                Log.Fatal("[READ ERROR] Error de conexión a MongoDB en GetTodosMPago.", ex);
                throw new MongoDBConnectionException(ex);
            }
            catch (MongoCommandException ex)
            {
                Log.Error("[READ ERROR] Error de comando en MongoDB en GetTodosMPago.", ex);
                throw new MongoDBCommandException(ex);
            }
            catch (MPagoNullRepositoryException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Error("[READ ERROR] Error desconocido al obtener todos los MPagos.", ex);
                throw new MPagoRepositoryException(ex);
            }
        }
        #endregion
    }
}
