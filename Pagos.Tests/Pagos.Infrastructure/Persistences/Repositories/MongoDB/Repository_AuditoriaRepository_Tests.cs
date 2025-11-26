using log4net;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Pagos.Domain.Exceptions;
using Pagos.Infrastructure.Configurations;
using Pagos.Infrastructure.Interfaces;
using Pagos.Infrastructure.Persistences.Repositories.MongoDB;
using Xunit;

namespace Pagos.Tests.Pagos.Infrastructure.Persistences.Repositories.MongoDB
{
    public class Repository_AuditoriaRepository_Tests
    {
        private readonly Mock<IMongoDatabase> MockMongoDb;
        private readonly Mock<IMongoCollection<BsonDocument>> MockAuditoriaCollection;
        private readonly Mock<ILog> MockLogger;

        private readonly AuditoriaRepository Repository;

        // Datos de prueba
        private const string TestUsuarioId = "user_123";
        private const string TestPagoId = "pago_123";
        private const string TestMPagoId = "mpago_123";
        private const string TestReservaId = "reserva_123";
        private const decimal TestMonto = 99.99m;
        private const string TestLevel = "INFO";
        private const string TestTipoPago = "PAGO_REGISTRADO";
        private const string TestTipoMPago = "MPAGO_REGISTRADO";
        private const string TestMensaje = "Mensaje de auditoría de prueba";

        public Repository_AuditoriaRepository_Tests()
        {
            // CONFIG: deben coincidir con AuditoriaDbConfig
            Environment.SetEnvironmentVariable("MONGODB_CNN", "mongodb://localhost:27017");
            Environment.SetEnvironmentVariable("MONGODB_NAME_AUDITORIAS", "test_database_auditorias");

            MockMongoDb = new Mock<IMongoDatabase>();
            MockAuditoriaCollection = new Mock<IMongoCollection<BsonDocument>>();
            MockLogger = new Mock<ILog>();

            MockMongoDb
                .Setup(d => d.GetCollection<BsonDocument>("auditoriaPagos", It.IsAny<MongoCollectionSettings>()))
                .Returns(MockAuditoriaCollection.Object);

            var mongoConfig = new AuditoriaDbConfig();
            mongoConfig.db = MockMongoDb.Object;

            Repository = new AuditoriaRepository(mongoConfig, MockLogger.Object);
        }

        // ===========================
        //   InsertarAuditoriaPago
        // ===========================

        #region InsertarAuditoriaPago_InvocacionExitosa_DebeInsertarDocumento()
        [Fact]
        public async Task InsertarAuditoriaPago_InvocacionExitosa_DebeInsertarDocumento()
        {
            // Arrange
            MockAuditoriaCollection
                .Setup(c => c.InsertOneAsync(
                        It.IsAny<BsonDocument>(),
                        It.IsAny<InsertOneOptions>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await Repository.InsertarAuditoriaPago(
                TestUsuarioId,
                TestLevel,
                TestTipoPago,
                TestPagoId,
                TestMPagoId,
                TestMonto,
                TestReservaId,
                TestMensaje
            );

            // Assert
            MockAuditoriaCollection.Verify(c => c.InsertOneAsync(
                    It.Is<BsonDocument>(doc =>
                        doc.Contains("_id") &&
                        doc["idUsuario"].AsString == TestUsuarioId &&
                        doc["idPago"].AsString == TestPagoId &&
                        doc["idMPago"].AsString == TestMPagoId &&
                        doc["idReserva"].AsString == TestReservaId &&
                        doc["monto"].AsDecimal == TestMonto &&
                        doc["level"].AsString == TestLevel &&
                        doc["tipo"].AsString == TestTipoPago &&
                        doc["mensaje"].AsString == TestMensaje),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region InsertarAuditoriaPago_ErrorInesperado_DebeLanzarAuditoriaRepositoryException()
        [Fact]
        public async Task InsertarAuditoriaPago_ErrorInesperado_DebeLanzarAuditoriaRepositoryException()
        {
            // Arrange
            MockAuditoriaCollection
                .Setup(c => c.InsertOneAsync(
                        It.IsAny<BsonDocument>(),
                        It.IsAny<InsertOneOptions>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Error simulado en InsertOneAsync"));

            // Act & Assert
            await Assert.ThrowsAsync<AuditoriaRepositoryException>(() =>
                Repository.InsertarAuditoriaPago(
                    TestUsuarioId,
                    TestLevel,
                    TestTipoPago,
                    TestPagoId,
                    TestMPagoId,
                    TestMonto,
                    TestReservaId,
                    TestMensaje
                ));
        }
        #endregion

        // ===========================
        //   InsertarAuditoriaMPago
        // ===========================

        #region InsertarAuditoriaMPago_InvocacionExitosa_DebeInsertarDocumento()
        [Fact]
        public async Task InsertarAuditoriaMPago_InvocacionExitosa_DebeInsertarDocumento()
        {
            // Arrange
            MockAuditoriaCollection
                .Setup(c => c.InsertOneAsync(
                        It.IsAny<BsonDocument>(),
                        It.IsAny<InsertOneOptions>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await Repository.InsertarAuditoriaMPago(
                TestUsuarioId,
                TestLevel,
                TestTipoMPago,
                TestMPagoId,
                TestMensaje
            );

            // Assert
            MockAuditoriaCollection.Verify(c => c.InsertOneAsync(
                    It.Is<BsonDocument>(doc =>
                        doc.Contains("_id") &&
                        doc["idUsuario"].AsString == TestUsuarioId &&
                        doc["idMPago"].AsString == TestMPagoId &&
                        doc["level"].AsString == TestLevel &&
                        doc["tipo"].AsString == TestTipoMPago &&
                        doc["mensaje"].AsString == TestMensaje),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        #endregion

        #region InsertarAuditoriaMPago_ErrorInesperado_DebeLanzarAuditoriaRepositoryException()
        [Fact]
        public async Task InsertarAuditoriaMPago_ErrorInesperado_DebeLanzarAuditoriaRepositoryException()
        {
            // Arrange
            MockAuditoriaCollection
                .Setup(c => c.InsertOneAsync(
                        It.IsAny<BsonDocument>(),
                        It.IsAny<InsertOneOptions>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Error simulado en InsertOneAsync"));

            // Act & Assert
            await Assert.ThrowsAsync<AuditoriaRepositoryException>(() =>
                Repository.InsertarAuditoriaMPago(
                    TestUsuarioId,
                    TestLevel,
                    TestTipoMPago,
                    TestMPagoId,
                    TestMensaje
                ));
        }
        #endregion
    }
}
