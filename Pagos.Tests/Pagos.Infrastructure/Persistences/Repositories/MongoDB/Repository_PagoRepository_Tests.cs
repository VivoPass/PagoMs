using log4net;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Servers;
using MongoDB.Driver.Core.Connections;
using Moq;
using Pagos.Domain.Aggregates;
using Pagos.Domain.Exceptions;
using Pagos.Domain.Interfaces;
using Pagos.Domain.ValueObjects;
using Pagos.Infrastructure.Configurations;
using Pagos.Infrastructure.Interfaces;
using Pagos.Infrastructure.Persistences.Repositories.MongoDB;
using Xunit;
using System.Net;

namespace Pagos.Tests.Pagos.Infrastructure.Persistences.Repositories.MongoDB
{
    public class Repository_PagoRepository_Tests
    {
        private readonly Mock<IMongoDatabase> MockMongoDb;
        private readonly Mock<IMongoCollection<BsonDocument>> MockPagoCollection;
        private readonly Mock<IPagoFactory> MockPagoFactory;
        private readonly Mock<IAuditoriaRepository> MockAuditoria;
        private readonly Mock<ILog> MockLogger;

        private readonly PagoRepository Repository;

        // Datos de prueba
        private readonly string TestPagoId = Guid.NewGuid().ToString();
        private readonly string TestUsuarioId = Guid.NewGuid().ToString();
        private readonly string TestMPagoId = Guid.NewGuid().ToString();
        private readonly string TestReservaId = Guid.NewGuid().ToString();
        private readonly string TestEventoId = Guid.NewGuid().ToString();
        private const decimal TestMonto = 150.75m;
        private readonly DateTime TestFechaPago = DateTime.UtcNow;
        private const string TestExternalPagoId = "pi_123456789";

        private readonly Pago ExpectedPago;
        private readonly BsonDocument TestBsonPago;


        private readonly string TestPagoId2 = Guid.NewGuid().ToString();
        private readonly string TestUsuarioId2 = Guid.NewGuid().ToString();
        private readonly string TestMPagoId2 = Guid.NewGuid().ToString();
        private readonly string TestReservaId2 = Guid.NewGuid().ToString();
        private readonly string TestEventoId2 = Guid.NewGuid().ToString();
        private const decimal TestMonto2 = 200.50m;
        private readonly DateTime TestFechaPago2 = DateTime.UtcNow.AddMinutes(-10);

        private readonly Pago ExpectedPago2;
        private readonly BsonDocument TestBsonPago2;


        public Repository_PagoRepository_Tests()
        {
            Environment.SetEnvironmentVariable("MONGODB_CNN", "mongodb://localhost:27017");
            Environment.SetEnvironmentVariable("MONGODB_NAME_PAGOS", "test_database_pagos");

            MockMongoDb = new Mock<IMongoDatabase>();
            MockPagoCollection = new Mock<IMongoCollection<BsonDocument>>();
            MockPagoFactory = new Mock<IPagoFactory>();
            MockAuditoria = new Mock<IAuditoriaRepository>();
            MockLogger = new Mock<ILog>();

            MockMongoDb
                .Setup(d => d.GetCollection<BsonDocument>("pagos", It.IsAny<MongoCollectionSettings>()))
                .Returns(MockPagoCollection.Object);

            var mongoConfig = new PagoDbConfig();
            mongoConfig.db = MockMongoDb.Object;

            Repository = new PagoRepository(
                mongoConfig,
                MockPagoFactory.Object,
                MockAuditoria.Object,
                MockLogger.Object
            );

            //Datos de dominio de prueba
            ExpectedPago = new Mock<Pago>(
                new VOIdPago(TestPagoId),
                new VOIdMPago(TestMPagoId),
                new VOIdUsuario(TestUsuarioId),
                new VOFechaPago(TestFechaPago),
                new VOMonto(TestMonto),
                new VOIdReserva(TestReservaId),
                new VOIdEvento(TestEventoId),
                new VOIdExternalPago(TestExternalPagoId)
            ).Object;

            TestBsonPago = new BsonDocument
            {
                { "_id", TestPagoId },
                { "idUsuario", TestUsuarioId },
                { "idMPago", TestMPagoId },
                { "idReserva", TestReservaId },
                { "idEvento", TestEventoId },
                { "monto", TestMonto },
                { "fechaPago", TestFechaPago },
                { "idExternalPago", TestExternalPagoId }
            };

            ExpectedPago2 = new Mock<Pago>(
                new VOIdPago(TestPagoId2),
                new VOIdMPago(TestMPagoId2),
                new VOIdUsuario(TestUsuarioId2),
                new VOFechaPago(TestFechaPago2),
                new VOMonto(TestMonto2),
                new VOIdReserva(TestReservaId2),
                new VOIdEvento(TestEventoId2),
                new VOIdExternalPago(TestExternalPagoId)
            ).Object;

            TestBsonPago2 = new BsonDocument
            {
                { "_id", TestPagoId2 },
                { "idUsuario", TestUsuarioId2 },
                { "idMPago", TestMPagoId2 },
                { "idReserva", TestReservaId2 },
                { "idEvento", TestEventoId2 },
                { "monto", TestMonto2 },
                { "fechaPago", TestFechaPago2 },
                { "idExternalPago", TestExternalPagoId }
            };
        }


        #region AgregarPago_InvocacionExitosa_DebeInsertarEnMongoYRegistrarAuditoria()
        [Fact]
        public async Task AgregarPago_InvocacionExitosa_DebeInsertarEnMongoYRegistrarAuditoria()
        {
            // Arrange
            MockPagoCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<BsonDocument>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            MockAuditoria
                .Setup(a => a.InsertarAuditoriaPago(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<decimal>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await Repository.AgregarPago(ExpectedPago);

            // Assert: se insertó 1 documento con el _id correcto
            MockPagoCollection.Verify(c => c.InsertOneAsync(
                    It.Is<BsonDocument>(doc =>
                        doc["_id"].AsString == ExpectedPago.IdPago.Valor &&
                        doc["idUsuario"].AsString == ExpectedPago.IdUsuario.Valor &&
                        doc["idMPago"].AsString == ExpectedPago.IdMPago.Valor),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // Se registró auditoría
            MockAuditoria.Verify(a => a.InsertarAuditoriaPago(
                    ExpectedPago.IdUsuario.Valor,
                    "INFO",
                    "PAGO_REGISTRADO",
                    ExpectedPago.IdPago.Valor,
                    ExpectedPago.IdMPago.Valor,
                    ExpectedPago.Monto.Valor,
                    ExpectedPago.IdReserva.Valor,
                    It.Is<string>(msg => msg.Contains(ExpectedPago.IdPago.Valor))),
                Times.Once);
        }
        #endregion

        #region AgregarPago_ErrorConexionMongo_DebeLanzarErrorConexionBd()
        [Fact]
        public async Task AgregarPago_ErrorConexionMongo_DebeLanzarErrorConexionBd()
        {
            // Arrange – simulamos MongoConnectionException
            var serverId = new ServerId(new ClusterId(), new DnsEndPoint("localhost", 27017));
            var connId = new ConnectionId(serverId);
            var mongoConn = new MongoConnectionException(connId, "Error de conexión simulado");

            MockPagoCollection
                .Setup(c => c.InsertOneAsync(
                        It.IsAny<BsonDocument>(),
                        It.IsAny<InsertOneOptions>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(mongoConn);

            // Act & Assert
            await Assert.ThrowsAsync<ErrorConexionBd>(
                () => Repository.AgregarPago(ExpectedPago)
            );
        }
        #endregion

        #region AgregarPago_ErrorInesperado_DebeLanzarPagoRepositoryException()
        [Fact]
        public async Task AgregarPago_ErrorInesperado_DebeLanzarPagoRepositoryException()
        {
            // Arrange – cualquier excepción genérica
            MockPagoCollection
                .Setup(c => c.InsertOneAsync(
                        It.IsAny<BsonDocument>(),
                        It.IsAny<InsertOneOptions>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Error inesperado"));

            // Act & Assert
            await Assert.ThrowsAsync<PagoRepositoryException>(
                () => Repository.AgregarPago(ExpectedPago)
            );
        }
        #endregion


        #region ActualizarIdPagoExterno_ActualizacionExitosa_DebeLlamarUpdateOneAsync()
        [Fact]
        public async Task ActualizarIdPagoExterno_ActualizacionExitosa_DebeLlamarUpdateOneAsync()
        {
            // Arrange
            var mockResult = new Mock<UpdateResult>();
            mockResult.SetupGet(r => r.ModifiedCount).Returns(1);

            MockPagoCollection
                .Setup(c => c.UpdateOneAsync(
                        It.IsAny<FilterDefinition<BsonDocument>>(),
                        It.IsAny<UpdateDefinition<BsonDocument>>(),
                        It.IsAny<UpdateOptions>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResult.Object);

            // Act
            await Repository.ActualizarIdPagoExterno(TestPagoId, TestExternalPagoId);

            // Assert
            MockPagoCollection.Verify(c => c.UpdateOneAsync(
                It.Is<FilterDefinition<BsonDocument>>(f => f != null),
                It.Is<UpdateDefinition<BsonDocument>>(u => u != null),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region ActualizarIdPagoExterno_SinModificaciones_NoDebeLanzarExcepcion()
        [Fact]
        public async Task ActualizarIdPagoExterno_SinModificaciones_NoDebeLanzarExcepcion()
        {
            // Arrange – ModifiedCount = 0
            var mockResult = new Mock<UpdateResult>();
            mockResult.SetupGet(r => r.ModifiedCount).Returns(0);

            MockPagoCollection
                .Setup(c => c.UpdateOneAsync(
                        It.IsAny<FilterDefinition<BsonDocument>>(),
                        It.IsAny<UpdateDefinition<BsonDocument>>(),
                        It.IsAny<UpdateOptions>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResult.Object);

            // Act
            await Repository.ActualizarIdPagoExterno(TestPagoId, TestExternalPagoId);

            // Assert: que al menos no reviente
            MockLogger.Verify(l => l.Warn(
                    It.Is<string>(msg => msg.Contains("No se encontró el Pago ID") ||
                                         msg.Contains("Documentos modificados"))),
                Times.Once);
        }
        #endregion

        #region ActualizarIdPagoExterno_ErrorConexionMongo_DebeLanzarErrorConexionBd()
        [Fact]
        public async Task ActualizarIdPagoExterno_ErrorConexionMongo_DebeLanzarErrorConexionBd()
        {
            // Arrange
            var serverId = new ServerId(new ClusterId(), new DnsEndPoint("localhost", 27017));
            var connId = new ConnectionId(serverId);
            var mongoConn = new MongoConnectionException(connId, "Error de conexión simulado");

            MockPagoCollection
                .Setup(c => c.UpdateOneAsync(
                        It.IsAny<FilterDefinition<BsonDocument>>(),
                        It.IsAny<UpdateDefinition<BsonDocument>>(),
                        It.IsAny<UpdateOptions>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(mongoConn);

            // Act & Assert
            await Assert.ThrowsAsync<ErrorConexionBd>(
                () => Repository.ActualizarIdPagoExterno(TestPagoId, TestExternalPagoId)
            );
        }
        #endregion

        #region ActualizarIdPagoExterno_ErrorInesperado_DebeLanzarPagoRepositoryException()
        [Fact]
        public async Task ActualizarIdPagoExterno_ErrorInesperado_DebeLanzarPagoRepositoryException()
        {
            // Arrange
            MockPagoCollection
                .Setup(c => c.UpdateOneAsync(
                        It.IsAny<FilterDefinition<BsonDocument>>(),
                        It.IsAny<UpdateDefinition<BsonDocument>>(),
                        It.IsAny<UpdateOptions>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Error inesperado"));

            // Act & Assert
            await Assert.ThrowsAsync<PagoRepositoryException>(
                () => Repository.ActualizarIdPagoExterno(TestPagoId, TestExternalPagoId)
            );
        }
        #endregion


        #region ObtenerPagoPorIdPago_PagoEncontrado_DebeRetornarPago()
        [Fact]
        public async Task ObtenerPagoPorIdPago_PagoEncontrado_DebeRetornarPago()
        {
            // Arrange: cursor que devuelve 1 documento y luego termina
            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            cursorMock.SetupGet(c => c.Current)
                .Returns(new List<BsonDocument> { TestBsonPago });

            MockPagoCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cursorMock.Object);

            MockPagoFactory
                .Setup(f => f.Load(
                    It.IsAny<VOIdPago>(),
                    It.IsAny<VOIdMPago>(),
                    It.IsAny<VOIdUsuario>(),
                    It.IsAny<VOFechaPago>(),
                    It.IsAny<VOMonto>(),
                    It.IsAny<VOIdReserva>(),
                    It.IsAny<VOIdEvento>(),
                    It.IsAny<VOIdExternalPago>()))
                .Returns(ExpectedPago);

            // Act
            var result = await Repository.ObtenerPagoPorIdPago(TestPagoId);

            // Assert
            Assert.NotNull(result);
            Assert.Same(ExpectedPago, result);
        }
        #endregion

        #region ObtenerPagoPorIdPago_PagoNoEncontrado_DebeLanzarPagoNullRepositoryException()
        [Fact]
        public async Task ObtenerPagoPorIdPago_PagoNoEncontrado_DebeLanzarPagoNullRepositoryException()
        {
            // cursor vacío
            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.Setup(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            MockPagoCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cursorMock.Object);

            await Assert.ThrowsAsync<PagoNullRepositoryException>(
                () => Repository.ObtenerPagoPorIdPago(TestPagoId)
            );

            MockPagoFactory.Verify(f => f.Load(
                    It.IsAny<VOIdPago>(),
                    It.IsAny<VOIdMPago>(),
                    It.IsAny<VOIdUsuario>(),
                    It.IsAny<VOFechaPago>(),
                    It.IsAny<VOMonto>(),
                    It.IsAny<VOIdReserva>(),
                    It.IsAny<VOIdEvento>(),
                    It.IsAny<VOIdExternalPago>()),
                Times.Never);
        }
        #endregion

        #region ObtenerPagoPorIdPago_ErrorConexionMongo_DebeLanzarErrorConexionBd()
        [Fact]
        public async Task ObtenerPagoPorIdPago_ErrorConexionMongo_DebeLanzarErrorConexionBd()
        {
            var serverId = new ServerId(new ClusterId(), new DnsEndPoint("localhost", 27017));
            var connId = new ConnectionId(serverId);
            var mongoConn = new MongoConnectionException(connId, "Error de conexión simulado");

            MockPagoCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(mongoConn);

            await Assert.ThrowsAsync<ErrorConexionBd>(
                () => Repository.ObtenerPagoPorIdPago(TestPagoId)
            );
        }
        #endregion

        #region ObtenerPagoPorIdPago_ErrorInesperado_DebeLanzarPagoRepositoryException()
        [Fact]
        public async Task ObtenerPagoPorIdPago_ErrorInesperado_DebeLanzarPagoRepositoryException()
        {
            MockPagoCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Error inesperado"));

            await Assert.ThrowsAsync<PagoRepositoryException>(
                () => Repository.ObtenerPagoPorIdPago(TestPagoId)
            );
        }
        #endregion


        #region ObtenerPagosPorIdUsuario_ConResultados_DebeRetornarListaDePagos()
        [Fact]
        public async Task ObtenerPagosPorIdUsuario_ConResultados_DebeRetornarListaDePagos()
        {
            // Arrange: simulamos 2 documentos en la colección
            var documentos = new List<BsonDocument> { TestBsonPago, TestBsonPago2 };

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true)
                      .ReturnsAsync(false);
            cursorMock.SetupGet(c => c.Current)
                      .Returns(documentos);

            MockPagoCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cursorMock.Object);

            // Cada documento mapeado a un Pago usando el factory
            MockPagoFactory
                .SetupSequence(f => f.Load(
                    It.IsAny<VOIdPago>(),
                    It.IsAny<VOIdMPago>(),
                    It.IsAny<VOIdUsuario>(),
                    It.IsAny<VOFechaPago>(),
                    It.IsAny<VOMonto>(),
                    It.IsAny<VOIdReserva>(),
                    It.IsAny<VOIdEvento>(),
                    It.IsAny<VOIdExternalPago>()))
                .Returns(ExpectedPago)
                .Returns(ExpectedPago2);

            // Act
            var result = await Repository.ObtenerPagosPorIdUsuario(TestUsuarioId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Same(ExpectedPago, result[0]);
            Assert.Same(ExpectedPago2, result[1]);
        }
        #endregion

        #region ObtenerPagosPorIdUsuario_SinResultados_DebeRetornarListaVacia()
        [Fact]
        public async Task ObtenerPagosPorIdUsuario_SinResultados_DebeRetornarListaVacia()
        {
            // Arrange: cursor vacío
            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.Setup(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(false);

            MockPagoCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cursorMock.Object);

            // Act
            var result = await Repository.ObtenerPagosPorIdUsuario(TestUsuarioId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            // Si no hay documentos, no debería invocar el factory
            MockPagoFactory.Verify(f => f.Load(
                It.IsAny<VOIdPago>(),
                It.IsAny<VOIdMPago>(),
                It.IsAny<VOIdUsuario>(),
                It.IsAny<VOFechaPago>(),
                It.IsAny<VOMonto>(),
                It.IsAny<VOIdReserva>(),
                It.IsAny<VOIdEvento>(),
                It.IsAny<VOIdExternalPago>()),
                Times.Never);
        }
        #endregion

        #region ObtenerPagosPorIdUsuario_ErrorInesperado_DebeLanzarPagoRepositoryException()
        [Fact]
        public async Task ObtenerPagosPorIdUsuario_ErrorInesperado_DebeLanzarPagoRepositoryException()
        {
            // Arrange: cualquier excepción desde Mongo
            MockPagoCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Error inesperado"));

            // Act & Assert
            await Assert.ThrowsAsync<PagoRepositoryException>(
                () => Repository.ObtenerPagosPorIdUsuario(TestUsuarioId)
            );
        }
        #endregion


        #region ObtenerPagosPorIdEvento_ConResultados_DebeRetornarListaDePagos()
        [Fact]
        public async Task ObtenerPagosPorIdEvento_ConResultados_DebeRetornarListaDePagos()
        {
            // Arrange: simulamos 2 documentos devueltos para ese evento
            var documentos = new List<BsonDocument> { TestBsonPago, TestBsonPago2 };

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true)
                      .ReturnsAsync(false);
            cursorMock.SetupGet(c => c.Current)
                      .Returns(documentos);

            MockPagoCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cursorMock.Object);

            MockPagoFactory
                .SetupSequence(f => f.Load(
                    It.IsAny<VOIdPago>(),
                    It.IsAny<VOIdMPago>(),
                    It.IsAny<VOIdUsuario>(),
                    It.IsAny<VOFechaPago>(),
                    It.IsAny<VOMonto>(),
                    It.IsAny<VOIdReserva>(),
                    It.IsAny<VOIdEvento>(),
                    It.IsAny<VOIdExternalPago>()))
                .Returns(ExpectedPago)
                .Returns(ExpectedPago2);

            // Act
            var result = await Repository.ObtenerPagosPorIdEvento(TestEventoId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Same(ExpectedPago, result[0]);
            Assert.Same(ExpectedPago2, result[1]);
        }
        #endregion

        #region ObtenerPagosPorIdEvento_SinResultados_DebeRetornarListaVacia()
        [Fact]
        public async Task ObtenerPagosPorIdEvento_SinResultados_DebeRetornarListaVacia()
        {
            // Arrange: cursor vacío
            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.Setup(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(false);

            MockPagoCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cursorMock.Object);

            // Act
            var result = await Repository.ObtenerPagosPorIdEvento(TestEventoId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            MockPagoFactory.Verify(f => f.Load(
                It.IsAny<VOIdPago>(),
                It.IsAny<VOIdMPago>(),
                It.IsAny<VOIdUsuario>(),
                It.IsAny<VOFechaPago>(),
                It.IsAny<VOMonto>(),
                It.IsAny<VOIdReserva>(),
                It.IsAny<VOIdEvento>(),
                It.IsAny<VOIdExternalPago>()),
                Times.Never);
        }
        #endregion

        #region ObtenerPagosPorIdEvento_ErrorInesperado_DebeLanzarPagoRepositoryException()
        [Fact]
        public async Task ObtenerPagosPorIdEvento_ErrorInesperado_DebeLanzarPagoRepositoryException()
        {
            // Arrange
            MockPagoCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Error inesperado"));

            // Act & Assert
            await Assert.ThrowsAsync<PagoRepositoryException>(
                () => Repository.ObtenerPagosPorIdEvento(TestEventoId)
            );
        }
        #endregion
    }
}
