using MongoDB.Bson;
using MongoDB.Driver;
using Pagos.Domain.Aggregates;
using Pagos.Domain.Entities;
using Pagos.Domain.Exceptions;
using Pagos.Domain.Interfaces;
using Pagos.Domain.ValueObjects;
using Pagos.Infrastructure.Configurations;
using System.Reflection.Metadata;

namespace Pagos.Infrastructure.Persistences.Repositories.MongoDB
{
    /// <summary>
    /// Repositorio para operaciones de escritura y consulta de medios de pago (TarjetaCredito)
    /// en la colección "mpago_write" de MongoDB.
    /// </summary>
    /// <remarks>
    /// Implementa <see cref="IMPagoRepository"/> y maneja la conversión entre documentos BSON
    /// y la entidad de dominio <see cref="TarjetaCredito"/>.
    /// </remarks>
    public class MPagoRepository : IMPagoRepository
    {
        private readonly IMongoCollection<BsonDocument> MPagoColexion;
        private readonly ITarjetaCreditoFactory TarjetaCreditoFactory;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="MPagoWriteRepository"/>.
        /// </summary>
        /// <param name="mongoConfig">
        /// Configuración de la base de datos de escritura para medios de pago
        /// (<see cref="MPagoWriteDbConfig"/>).
        /// </param>
        public MPagoRepository(PagoDbConfig mongoConfig, ITarjetaCreditoFactory tarjetaCreditoFactory)
        {
            MPagoColexion = mongoConfig.db.GetCollection<BsonDocument>("mpagos");
            TarjetaCreditoFactory = tarjetaCreditoFactory;
        }

        #region ObtenerMPagoPorId(string idMPago)
        /// <summary>
        /// Recupera una tarjeta de crédito por su identificador único.
        /// </summary>
        /// <param name="idMPago">Identificador de la tarjeta de crédito.</param>
        /// <returns>
        /// Instancia de <see cref="TarjetaCredito"/> si existe el documento;
        /// de lo contrario, lanza <see cref="MPagoNullRepositoryException"/>.
        /// </returns>
        /// <exception cref="MPagoWriteRepositoryException">
        /// Se lanza si ocurre un error durante la consulta o conversión del documento.
        /// </exception>
        public async Task<TarjetaCredito> ObtenerMPagoPorId(string idMPago)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", idMPago);
                var documento = await MPagoColexion.Find(filter).FirstOrDefaultAsync();
                if (documento == null)
                    throw new MPagoNullRepositoryException();

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


                return mpago;
            }
            catch (MPagoNullRepositoryException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new MPagoWriteRepositoryException(ex);
            }
        }
        #endregion

        #region Task<List<TarjetaCredito>> ObtenerMPagoPorIdUsuario(string idPostor)
        /// <summary>
        /// Recupera todas las tarjetas de crédito asociadas a un postor específico.
        /// </summary>
        /// <param name="idPostor">Identificador del postor.</param>
        /// <returns>
        /// Lista de <see cref="TarjetaCredito"/> encontradas; puede estar vacía si no hay registros.
        /// </returns>
        /// <exception cref="MPagoWriteRepositoryException">
        /// Se lanza si ocurre un error durante la consulta o conversión de documentos.
        /// </exception>
        public async Task<List<TarjetaCredito>> ObtenerMPagoPorIdUsuario(string idUsuario)
        {
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
                return mPagos;
            }
            catch (Exception ex)
            {
                throw new MPagoWriteRepositoryException(ex);
            }
        }
        #endregion

        #region AgregarMPago(TarjetaCredito mPago)
        /// <summary>
        /// Inserta una nueva tarjeta de crédito en la colección.
        /// </summary>
        /// <param name="mPago">Entidad de dominio <see cref="TarjetaCredito"/> a agregar.</param>
        /// <returns>
        /// La misma instancia de <see cref="TarjetaCredito"/> insertada en la base de datos.
        /// </returns>
        /// <exception cref="MPagoWriteRepositoryException">
        /// Se lanza si ocurre un error durante la inserción del documento.
        /// </exception>
        public async Task<TarjetaCredito> AgregarMPago(TarjetaCredito mPago)
        {
            try
            {
                var documento = new BsonDocument
            {
                { "_id", mPago.IdMPago.Valor },
                { "idUsuario", mPago.IdUsuario.Valor },
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
                return mPago;
            }
            catch (Exception ex)
            {
                throw new MPagoWriteRepositoryException(ex);
            }
        }
        #endregion

        #region ActualizarPredeterminadoTrueMPago(string idMPago)
        /// <summary>
        /// Marca una tarjeta de crédito como predeterminada estableciendo su campo "predeterminado" en true.
        /// </summary>
        /// <param name="idMPago">Identificador de la tarjeta a actualizar.</param>
        /// <returns>Tarea que completa la operación de actualización.</returns>
        /// <exception cref="MPagoWriteRepositoryException">
        /// Se lanza si ocurre un error durante la actualización del documento.
        /// </exception>
        public async Task ActualizarPredeterminadoTrueMPago(string idMPago)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", idMPago);
                var update = Builders<BsonDocument>.Update.Set("predeterminado", true);
                await MPagoColexion.UpdateOneAsync(filter, update);
            }
            catch (Exception ex)
            {
                throw new MPagoWriteRepositoryException(ex);
            }
        }
        #endregion

        #region ActualizarPredeterminadoFalseMPago(string idMPago)
        /// <summary>
        /// Desmarca una tarjeta de crédito como predeterminada estableciendo su campo "predeterminado" en false.
        /// </summary>
        /// <param name="idMPago">Identificador de la tarjeta a actualizar.</param>
        /// <returns>Tarea que completa la operación de actualización.</returns>
        /// <exception cref="MPagoWriteRepositoryException">
        /// Se lanza si ocurre un error durante la actualización del documento.
        /// </exception>
        public async Task ActualizarPredeterminadoFalseMPago(string idMPago)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", idMPago);
                var update = Builders<BsonDocument>.Update.Set("predeterminado", false);
                await MPagoColexion.UpdateOneAsync(filter, update);
            }
            catch (Exception ex)
            {
                throw new MPagoWriteRepositoryException(ex);
            }
        }
        #endregion

        #region EliminarMPago(string idMPago)
        /// <summary>
        /// Elimina una tarjeta de crédito de la colección por su identificador.
        /// </summary>
        /// <param name="idMPago">Identificador de la tarjeta a eliminar.</param>
        /// <returns>Tarea que completa la operación de eliminación.</returns>
        /// <exception cref="MPagoWriteRepositoryException">
        /// Se lanza si ocurre un error durante la eliminación del documento.
        /// </exception>
        public async Task EliminarMPago(string idMPago)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", idMPago);
                await MPagoColexion.DeleteOneAsync(filter);
            }
            catch (Exception ex)
            {
                throw new MPagoWriteRepositoryException(ex);
            }
        }
        #endregion

        #region GetTodosMPago()
        /// <summary>
        /// Recupera todos los medios de pago disponibles en la colección.
        /// </summary>
        /// <returns>
        /// Lista de documentos BSON; lanza <see cref="MPagoNullRepositoryException"/> si no hay registros.
        /// </returns>
        /// <exception cref="MongoDBConnectionException">
        /// Se lanza si falla la conexión con MongoDB.
        /// </exception>
        /// <exception cref="MongoDBCommandException">
        /// Se lanza si ocurre un error al ejecutar el comando en MongoDB.
        /// </exception>
        /// <exception cref="MPagoReadRepositoryException">
        /// Se lanza para cualquier otro error inesperado.
        /// </exception>
        public async Task<List<TarjetaCredito>> GetTodosMPago()
        {
            try
            {
                var mpagos = await MPagoColexion.Find(_ => true).ToListAsync();
                if (mpagos == null || mpagos.Count == 0)
                {
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
                return MPagos;
            }
            catch (MongoConnectionException ex)
            {
                throw new MongoDBConnectionException(ex);
            }
            catch (MongoCommandException ex)
            {
                throw new MongoDBCommandException(ex);
            }
            catch (Exception ex)
            {
                throw new MPagoReadRepositoryException(ex);
            }
        }
        #endregion
    }

}
