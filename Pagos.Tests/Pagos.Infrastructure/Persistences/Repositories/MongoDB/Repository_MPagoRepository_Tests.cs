using log4net;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Pagos.Domain.Aggregates;
using Pagos.Domain.Entities;
using Pagos.Domain.Exceptions;
using Pagos.Domain.Interfaces;
using Pagos.Domain.ValueObjects;
using Pagos.Infrastructure.Configurations;
using Pagos.Infrastructure.Interfaces;
using Pagos.Infrastructure.Persistences.Repositories.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Pagos.Tests.Pagos.Infrastructure.Persistences.Repositories.MongoDB
{
    public class Repository_MPagoRepository_Tests
    {
        private readonly Mock<IMongoDatabase> MockMongoDb;
        private readonly Mock<IMongoCollection<BsonDocument>> MockPagoCollection;
        private readonly Mock<ITarjetaCreditoFactory> MockPagoFactory;
        private readonly Mock<IAuditoriaRepository> MockAuditoria;
        private readonly Mock<ILog> MockLogger;
        private readonly MPagoRepository Repository;

        // Datos de prueba
        private readonly string TestUsuarioId = Guid.NewGuid().ToString();
        private readonly string TestMPagoId = Guid.NewGuid().ToString();

        private readonly TarjetaCredito ExpectedPago;
        private readonly BsonDocument TestBsonPago;

        private readonly string TestUsuarioId2 = Guid.NewGuid().ToString();
        private readonly string TestMPagoId2 = Guid.NewGuid().ToString();

        private readonly TarjetaCredito ExpectedPago2;
        private readonly BsonDocument TestBsonPago2;

        private readonly List<BsonDocument> ListaBsonDocuments;
        private readonly List<TarjetaCredito> ListaMPagos;

        public Repository_MPagoRepository_Tests()
        {
            Environment.SetEnvironmentVariable("MONGODB_CNN", "mongodb://localhost:27017");
            Environment.SetEnvironmentVariable("MONGODB_NAME_PAGOS", "test_database_pagos");

            MockMongoDb = new Mock<IMongoDatabase>();
            MockPagoCollection = new Mock<IMongoCollection<BsonDocument>>();
            MockPagoFactory = new Mock<ITarjetaCreditoFactory>();
            MockAuditoria = new Mock<IAuditoriaRepository>();
            MockLogger = new Mock<ILog>();

            MockMongoDb
                .Setup(d => d.GetCollection<BsonDocument>("mpagos", It.IsAny<MongoCollectionSettings>()))
                .Returns(MockPagoCollection.Object);

            var mongoConfig = new PagoDbConfig();
            mongoConfig.db = MockMongoDb.Object;

            Repository = new MPagoRepository(
                mongoConfig,
                MockPagoFactory.Object,
                MockAuditoria.Object,
                MockLogger.Object
            );

            //Datos de dominio de prueba
            ExpectedPago = new Mock<TarjetaCredito>(
                new VOIdMPago(TestMPagoId),
                new VOIdUsuario(TestUsuarioId),
                new VOIdMPagoStripe("pm_123456789"),
                new VOIdClienteStripe("cus_987654321"),
                new VOMarca("visa"),
                new VOMesExpiracion(12),
                new VOAnioExpiracion(2030),
                new VOUltimos4("4242"),
                new VOFechaRegistro(DateTime.UtcNow),
                new VOPredeterminado(true)
            ).Object;

            TestBsonPago = new BsonDocument
            {
                { "_id", ExpectedPago.IdMPago.Valor },
                { "idUsuario", ExpectedPago.IdMPago.Valor },
                { "idMPagoStripe", ExpectedPago.IdMPagoStripe.Valor },
                { "idClienteStripe", ExpectedPago.IdClienteStripe.Valor },
                { "marca", ExpectedPago.Marca.Valor },
                { "mesExpiracion", ExpectedPago.MesExpiracion.Valor },
                { "anioExpiracion", ExpectedPago.AnioExpiracion.Valor },
                { "ultimos4", ExpectedPago.Ultimos4.Valor },
                { "fechaRegistro", ExpectedPago.FechaRegistro.Valor.ToLocalTime() },
                { "predeterminado", ExpectedPago.Predeterminado.Valor }
            };

            ExpectedPago2 = new Mock<TarjetaCredito>(
                new VOIdMPago(TestMPagoId2),
                new VOIdUsuario(TestUsuarioId2),
                new VOIdMPagoStripe("pm_987654321"),
                new VOIdClienteStripe("cus_123456789"),
                new VOMarca("mastercard"),
                new VOMesExpiracion(10),
                new VOAnioExpiracion(2031),
                new VOUltimos4("4444"),
                new VOFechaRegistro(DateTime.UtcNow),
                new VOPredeterminado(true)
            ).Object;

            TestBsonPago2 = new BsonDocument
            {
                { "_id", ExpectedPago2.IdMPago.Valor },
                { "idUsuario", ExpectedPago2.IdMPago.Valor },
                { "idMPagoStripe", ExpectedPago2.IdMPagoStripe.Valor },
                { "idClienteStripe", ExpectedPago2.IdClienteStripe.Valor },
                { "marca", ExpectedPago2.Marca.Valor },
                { "mesExpiracion", ExpectedPago2.MesExpiracion.Valor },
                { "anioExpiracion", ExpectedPago2.AnioExpiracion.Valor },
                { "ultimos4", ExpectedPago2.Ultimos4.Valor },
                { "fechaRegistro", ExpectedPago2.FechaRegistro.Valor.ToLocalTime() },
                { "predeterminado", ExpectedPago2.Predeterminado.Valor }
            };

            ListaBsonDocuments = new List<BsonDocument> { TestBsonPago, TestBsonPago2 };
            ListaMPagos = new List<TarjetaCredito> { ExpectedPago, ExpectedPago2 };

        }

        #region ObtenerMPagoPorId_MPagoEncontrado_RetornaTarjetaCredito()
        [Fact]
        public async Task ObtenerMPagoPorId_MPagoEncontrado_RetornaTarjetaCredito()
        {
            // ARRANGE

            MockPagoFactory.Setup(f => f.Load(
                    It.IsAny<VOIdMPago>(), It.IsAny<VOIdUsuario>(), It.IsAny<VOIdMPagoStripe>(),
                    It.IsAny<VOIdClienteStripe>(), It.IsAny<VOMarca>(), It.IsAny<VOMesExpiracion>(),
                    It.IsAny<VOAnioExpiracion>(), It.IsAny<VOUltimos4>(), It.IsAny<VOFechaRegistro>(),
                    It.IsAny<VOPredeterminado>()))
                .Returns(ExpectedPago);

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(true).ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument> { TestBsonPago });

            MockPagoCollection.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                    default))
                .ReturnsAsync(cursorMock.Object);

            // ACT
            var result = await Repository.ObtenerMPagoPorId(TestMPagoId);

            // ASSERT
            Assert.Equal(TestMPagoId, result.IdMPago.Valor);
        }
        #endregion

        #region ObtenerMPagoPorId_MPagoNoEncontrado_ThrowsException()
        [Fact]
        public async Task ObtenerMPagoPorId_MPagoNoEncontrado_ThrowsException()
        {
            // ARRANGE

            MockPagoFactory.Setup(f => f.Load(
                    It.IsAny<VOIdMPago>(), It.IsAny<VOIdUsuario>(), It.IsAny<VOIdMPagoStripe>(),
                    It.IsAny<VOIdClienteStripe>(), It.IsAny<VOMarca>(), It.IsAny<VOMesExpiracion>(),
                    It.IsAny<VOAnioExpiracion>(), It.IsAny<VOUltimos4>(), It.IsAny<VOFechaRegistro>(),
                    It.IsAny<VOPredeterminado>()))
                .Returns(ExpectedPago);

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument> { TestBsonPago });

            MockPagoCollection.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                    default))
                .ReturnsAsync(cursorMock.Object);

            // ACT & ASSERT
            await Assert.ThrowsAsync<MPagoNullRepositoryException>(() => Repository.ObtenerMPagoPorId(TestMPagoId));
        }
        #endregion

        #region ObtenerMPagoPorId_Error_ThrowsException()
        [Fact]
        public async Task ObtenerMPagoPorId_Error_ThrowsException()
        {
            // ARRANGE
            var mongoException = new MongoException("Error de conexión");

            MockPagoFactory.Setup(f => f.Load(
                    It.IsAny<VOIdMPago>(), It.IsAny<VOIdUsuario>(), It.IsAny<VOIdMPagoStripe>(),
                    It.IsAny<VOIdClienteStripe>(), It.IsAny<VOMarca>(), It.IsAny<VOMesExpiracion>(),
                    It.IsAny<VOAnioExpiracion>(), It.IsAny<VOUltimos4>(), It.IsAny<VOFechaRegistro>(),
                    It.IsAny<VOPredeterminado>()))
                .Returns(ExpectedPago);

            MockPagoCollection.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                    default))
                .ThrowsAsync(mongoException);

            // ACT & ASSERT
            await Assert.ThrowsAsync<MPagoRepositoryException>(() => Repository.ObtenerMPagoPorId(TestMPagoId));
        }
        #endregion


        #region ObtenerMPagoPorIdUsuario_MPagoEncontrado_RetornaTarjetaCredito()
        [Fact]
        public async Task ObtenerMPagoPorIdUsuario_MPagoEncontrado_RetornaTarjetaCredito()
        {
            // ARRANGE

            MockPagoFactory.Setup(f => f.Load(
                    It.IsAny<VOIdMPago>(), It.IsAny<VOIdUsuario>(), It.IsAny<VOIdMPagoStripe>(),
                    It.IsAny<VOIdClienteStripe>(), It.IsAny<VOMarca>(), It.IsAny<VOMesExpiracion>(),
                    It.IsAny<VOAnioExpiracion>(), It.IsAny<VOUltimos4>(), It.IsAny<VOFechaRegistro>(),
                    It.IsAny<VOPredeterminado>()))
                .Returns(ExpectedPago);

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(true).ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument> { TestBsonPago });

            MockPagoCollection.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                    default))
                .ReturnsAsync(cursorMock.Object);

            // ACT
            var result = await Repository.ObtenerMPagoPorIdUsuario(TestUsuarioId);

            // ASSERT
            Assert.Equal(ExpectedPago, result.First());
        }
        #endregion

        #region ObtenerMPagoPorIdUsuario_MPagoNoEncontrado_RetornaListaVacia()
        [Fact]
        public async Task ObtenerMPagoPorIdUsuario_MPagoNoEncontrado_RetornaListaVacia()
        {
            // ARRANGE

            MockPagoFactory.Setup(f => f.Load(
                    It.IsAny<VOIdMPago>(), It.IsAny<VOIdUsuario>(), It.IsAny<VOIdMPagoStripe>(),
                    It.IsAny<VOIdClienteStripe>(), It.IsAny<VOMarca>(), It.IsAny<VOMesExpiracion>(),
                    It.IsAny<VOAnioExpiracion>(), It.IsAny<VOUltimos4>(), It.IsAny<VOFechaRegistro>(),
                    It.IsAny<VOPredeterminado>()))
                .Returns(ExpectedPago);

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument> {});

            MockPagoCollection.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                    default))
                .ReturnsAsync(cursorMock.Object);

            // ACT
            var result = await Repository.ObtenerMPagoPorIdUsuario(TestUsuarioId);

            // ASSERT
            Assert.Empty(result);
        }
        #endregion

        #region ObtenerMPagoPorIdUsuario_Error_ThrowsException()
        [Fact]
        public async Task ObtenerMPagoPorIdUsuario_Error_ThrowsException()
        {
            // ARRANGE
            var mongoException = new MongoException("Error de conexión");

            MockPagoFactory.Setup(f => f.Load(
                    It.IsAny<VOIdMPago>(), It.IsAny<VOIdUsuario>(), It.IsAny<VOIdMPagoStripe>(),
                    It.IsAny<VOIdClienteStripe>(), It.IsAny<VOMarca>(), It.IsAny<VOMesExpiracion>(),
                    It.IsAny<VOAnioExpiracion>(), It.IsAny<VOUltimos4>(), It.IsAny<VOFechaRegistro>(),
                    It.IsAny<VOPredeterminado>()))
                .Returns(ExpectedPago);

            MockPagoCollection.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                    default))
                .ThrowsAsync(mongoException);

            // ACT & ASSERT
            await Assert.ThrowsAsync<MPagoRepositoryException>(() => Repository.ObtenerMPagoPorIdUsuario(TestUsuarioId));
        }
        #endregion


        #region AgregarMPago_Exitos_RetornaIdMPago()
        [Fact]
        public async Task AgregarMPago_Exitos_RetornaIdMPago()
        {
            // ARRANGE
            MockAuditoria.Setup(a => a.InsertarAuditoriaMPago(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            MockPagoCollection.Setup(c => c.InsertOneAsync(
                It.IsAny<BsonDocument>(), It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // ACT
            var result = await Repository.AgregarMPago(ExpectedPago);

            // ASSERT
            Assert.Equal(TestMPagoId, result.IdMPago.Valor);
            MockAuditoria.Verify(a => a.InsertarAuditoriaMPago(
                    ExpectedPago.IdUsuario.Valor, "INFO", "MPAGO_REGISTRADO",
                    ExpectedPago.IdMPago.Valor, It.Is<string>(msg => msg.Contains(ExpectedPago.IdMPago.Valor))),
                Times.Once);
        }
        #endregion

        #region AgregarMPago_Fallo_ThrowsException()
        [Fact]
        public async Task AgregarMPago_Fallo_ThrowsException()
        {
            // ARRANGE
            var mongoException = new MongoException("Error de conexión");

            MockPagoCollection.Setup(c => c.InsertOneAsync(
                It.IsAny<BsonDocument>(), It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>())).ThrowsAsync(mongoException);

            // ACT & ASSERT
            await Assert.ThrowsAsync<MPagoRepositoryException>(() => Repository.AgregarMPago(ExpectedPago));
            MockAuditoria.Verify(a => a.InsertarAuditoriaMPago(
                    ExpectedPago.IdUsuario.Valor, "INFO", "MPAGO_REGISTRADO",
                    ExpectedPago.IdMPago.Valor, It.Is<string>(msg => msg.Contains(ExpectedPago.IdMPago.Valor))),
                Times.Never);
        }
        #endregion


        #region ActualizarPredeterminadoTrueMPago_MPagoEncontrado_ActualizaYAudita()
        [Fact]
        public async Task ActualizarPredeterminadoTrueMPago_MPagoEncontrado_ActualizaYAudita()
        {
            //ARRANGE
            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument> { TestBsonPago });

            MockPagoCollection.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                default)).ReturnsAsync(cursorMock.Object);

            var mockUpdateResult = Mock.Of<UpdateResult>(r => r.ModifiedCount == 1);
            UpdateDefinition<BsonDocument> capturedUpdate = null;

            MockPagoCollection.Setup(c => c.UpdateOneAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<UpdateDefinition<BsonDocument>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                // Captura el argumento de actualización antes de devolver el mock result
                .Callback<FilterDefinition<BsonDocument>, UpdateDefinition<BsonDocument>, UpdateOptions, CancellationToken>(
                    (filter, update, options, token) => capturedUpdate = update)
                .ReturnsAsync(mockUpdateResult);

            MockAuditoria.Setup(a => a.InsertarAuditoriaMPago(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // ACT
            await Repository.ActualizarPredeterminadoTrueMPago(TestMPagoId);

            // ASSERT
            MockPagoCollection.Verify(c => c.UpdateOneAsync(
                It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<UpdateDefinition<BsonDocument>>(),
                It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>()), Times.Once);

            MockAuditoria.Verify(a => a.InsertarAuditoriaMPago(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
        #endregion

        #region ActualizarPredeterminadoTrueMPago_MPagoNoEncontrado_NoAudita()
        [Fact]
        public async Task ActualizarPredeterminadoTrueMPago_MPagoNoEncontrado_NoAudita()
        {
            // ARRANGE
            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument> { });

            MockPagoCollection.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                default)).ReturnsAsync(cursorMock.Object);

            var mockUpdateResult = Mock.Of<UpdateResult>(r => r.ModifiedCount == 0);

            MockPagoCollection.Setup(c => c.UpdateOneAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<UpdateDefinition<BsonDocument>>(),
                    It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockUpdateResult);

            // ACT
            await Repository.ActualizarPredeterminadoTrueMPago(TestMPagoId);

            // ASSERT
            MockPagoCollection.Verify(c => c.UpdateOneAsync(
                It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<UpdateDefinition<BsonDocument>>(),
                It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>()), Times.Once);

            MockAuditoria.Verify(a => a.InsertarAuditoriaMPago(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
        #endregion

        #region ActualizarPredeterminadoTrueMPago_ErrorEnUpdate_LanzaExcepcionYNoAudita()
        [Fact]
        public async Task ActualizarPredeterminadoTrueMPago_ErrorEnUpdate_LanzaExcepcionYNoAudita()
        {
            // ARRANGE
            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default)).ReturnsAsync(false);
            MockPagoCollection.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                    default)).ReturnsAsync(cursorMock.Object);

            var expectedException = new Exception("Simulated MongoDB update failure.");

            MockPagoCollection.Setup(c => c.UpdateOneAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<UpdateDefinition<BsonDocument>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            MockAuditoria.Setup(a => a.InsertarAuditoriaMPago(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<MPagoRepositoryException>(() => Repository.ActualizarPredeterminadoTrueMPago(TestMPagoId));

            Assert.IsType<Exception>(ex.InnerException);

            MockAuditoria.Verify(a => a.InsertarAuditoriaMPago(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
        #endregion


        #region ActualizarPredeterminadoTrueMPago_MPagoEncontrado_ActualizaYAudita()
        [Fact]
        public async Task ActualizarPredeterminadoFalseMPago_MPagoEncontrado_ActualizaYAudita()
        {
            // ARRANGE
            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument> { TestBsonPago });

            MockPagoCollection.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                    default)).ReturnsAsync(cursorMock.Object);

            var mockUpdateResult = Mock.Of<UpdateResult>(r => r.ModifiedCount == 1);
            UpdateDefinition<BsonDocument> capturedUpdate = null;

            MockPagoCollection.Setup(c => c.UpdateOneAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<UpdateDefinition<BsonDocument>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                // Captura el argumento de actualización antes de devolver el mock result
                .Callback<FilterDefinition<BsonDocument>, UpdateDefinition<BsonDocument>, UpdateOptions, CancellationToken>(
                    (filter, update, options, token) => capturedUpdate = update)
                .ReturnsAsync(mockUpdateResult);

            MockAuditoria.Setup(a => a.InsertarAuditoriaMPago(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // ACT
            await Repository.ActualizarPredeterminadoFalseMPago(TestMPagoId);

            // ASSERT
            MockPagoCollection.Verify(c => c.UpdateOneAsync(
                It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<UpdateDefinition<BsonDocument>>(),
                It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>()), Times.Once);

            MockAuditoria.Verify(a => a.InsertarAuditoriaMPago(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
        #endregion

        #region ActualizarPredeterminadoFalseMPago_MPagoNoEncontrado_NoAudita()
        [Fact]
        public async Task ActualizarPredeterminadoFalseMPago_MPagoNoEncontrado_NoAudita()
        {
            // ARRANGE
            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument> { });

            MockPagoCollection.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                default)).ReturnsAsync(cursorMock.Object);

            var mockUpdateResult = Mock.Of<UpdateResult>(r => r.ModifiedCount == 0);

            MockPagoCollection.Setup(c => c.UpdateOneAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<UpdateDefinition<BsonDocument>>(),
                    It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockUpdateResult);

            // ACT
            await Repository.ActualizarPredeterminadoFalseMPago(TestMPagoId);

            // ASSERT
            MockPagoCollection.Verify(c => c.UpdateOneAsync(
                It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<UpdateDefinition<BsonDocument>>(),
                It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>()), Times.Once);

            MockAuditoria.Verify(a => a.InsertarAuditoriaMPago(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
        #endregion

        #region ActualizarPredeterminadoFalseMPago_ErrorEnUpdate_LanzaExcepcionYNoAudita()
        [Fact]
        public async Task ActualizarPredeterminadoFalseMPago_ErrorEnUpdate_LanzaExcepcionYNoAudita()
        {
            // ARRANGE
            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default)).ReturnsAsync(false);
            MockPagoCollection.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                    default)).ReturnsAsync(cursorMock.Object);

            var expectedException = new Exception("Simulated MongoDB update failure.");

            MockPagoCollection.Setup(c => c.UpdateOneAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<UpdateDefinition<BsonDocument>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            MockAuditoria.Setup(a => a.InsertarAuditoriaMPago(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<MPagoRepositoryException>(() => Repository.ActualizarPredeterminadoFalseMPago(TestMPagoId));

            Assert.IsType<Exception>(ex.InnerException);

            MockAuditoria.Verify(a => a.InsertarAuditoriaMPago(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
        #endregion


        #region EliminarMPago_EncontradoYEliminado_AuditaCorrectamente()
        [Fact]
        public async Task EliminarMPago_EncontradoYEliminado_AuditaCorrectamente()
        {
            // ARRANGE
            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument> { TestBsonPago });

            MockPagoCollection.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                default)).ReturnsAsync(cursorMock.Object);

            var mockDeleteResult = Mock.Of<DeleteResult>(r => r.DeletedCount == 1);
            MockPagoCollection.Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockDeleteResult);

            // ACT
            await Repository.EliminarMPago(TestMPagoId);

            // ASSERT
            MockPagoCollection.Verify(c => c.DeleteOneAsync(
                It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<CancellationToken>()), Times.Once);

            MockAuditoria.Verify(a => a.InsertarAuditoriaMPago(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
        #endregion

        #region EliminarMPago_NoEncontradoEnFind_LanzaExcepcion()
        [Fact]
        public async Task EliminarMPago_NoEncontradoEnFind_LanzaExcepcion()
        {
            // ARRANGE
            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument> { });

            MockPagoCollection.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                default)).ReturnsAsync(cursorMock.Object);

            MockPagoCollection.Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<DeleteResult>(r => r.DeletedCount == 0));

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<MPagoRepositoryException>(() => Repository.EliminarMPago(TestMPagoId));
            Assert.IsType<NullReferenceException>(ex.InnerException);

            MockAuditoria.Verify(a => a.InsertarAuditoriaMPago(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
        #endregion

        #region EliminarMPago_ErrorEnDeleteOne_LanzaExcepcionYNoAudita()
        [Fact]
        public async Task EliminarMPago_ErrorEnDeleteOne_LanzaExcepcionYNoAudita()
        {
            // ARRANGE
            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument> { TestBsonPago });

            MockPagoCollection.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                default)).ReturnsAsync(cursorMock.Object);

            var expectedException = new MongoException("Simulated MongoDB delete failure.");
            MockPagoCollection.Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<MPagoRepositoryException>(() => Repository.EliminarMPago(TestMPagoId));
            Assert.IsType<MongoException>(ex.InnerException);

            MockAuditoria.Verify(a => a.InsertarAuditoriaMPago(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
        #endregion

        #region EliminarMPago_EncontradoPeroNoEliminado_NoAudita()
        [Fact]
        public async Task EliminarMPago_EncontradoPeroNoEliminado_NoAudita()
        {
            // ARRANGE
            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(new List<BsonDocument> { TestBsonPago });

            MockPagoCollection.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                default)).ReturnsAsync(cursorMock.Object);

            var mockDeleteResult = Mock.Of<DeleteResult>(r => r.DeletedCount == 0);

            MockPagoCollection.Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockDeleteResult);

            // ACT
            await Repository.EliminarMPago(TestMPagoId);

            // ASSERT
            MockPagoCollection.Verify(c => c.DeleteOneAsync(
                It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<CancellationToken>()), Times.Once);

            MockAuditoria.Verify(a => a.InsertarAuditoriaMPago(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
        #endregion


        #region GetTodosMPago_Exito_RetornaListaMPagos()
        [Fact]
        public async Task GetTodosMPago_Exito_RetornaListaMPagos()
        {
            // ARRANGE
            MockPagoFactory.SetupSequence(f => f.Load(
                    It.IsAny<VOIdMPago>(), It.IsAny<VOIdUsuario>(), It.IsAny<VOIdMPagoStripe>(),
                    It.IsAny<VOIdClienteStripe>(), It.IsAny<VOMarca>(), It.IsAny<VOMesExpiracion>(),
                    It.IsAny<VOAnioExpiracion>(), It.IsAny<VOUltimos4>(), It.IsAny<VOFechaRegistro>(),
                    It.IsAny<VOPredeterminado>()))
                .Returns(ExpectedPago)
                .Returns(ExpectedPago2);

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(true).ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(ListaBsonDocuments);

            MockPagoCollection.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                    default))
                .ReturnsAsync(cursorMock.Object);

            // ACT
            var result = await Repository.GetTodosMPago();

            // ASSERT
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r == ExpectedPago);
            Assert.Contains(result, r => r == ExpectedPago2);
        }
        #endregion

        #region GetTodosMPago_ColeccionVacia_DebeRetornarListaVacia()
        [Fact]
        public async Task GetTodosMPago_ColeccionVacia_ThrowsException()
        {
            // ARRANGE
            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(true).ReturnsAsync(false);

            MockPagoCollection.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                    default))
                .ReturnsAsync(cursorMock.Object);

            // ACT & ASSERT
            await Assert.ThrowsAsync<MPagoNullRepositoryException>(() => Repository.GetTodosMPago());
        }
        #endregion

        #region GetTodosMPago_MapeoFallaPorBSONInvalido_RelanzaComoMPagoRepositoryException()
        [Fact]
        public async Task GetTodosMPago_MapeoFallaPorBSONInvalido_RelanzaComoMPagoRepositoryException()
        {
            // ARRANGE
            var invalidBsonDoc = new List<BsonDocument>
            {
                new BsonDocument
                {
                    { "_id", "id1" }, /* FALTA idUsuario */ { "idMPagoStripe", "mp_stripe1" },
                    { "idClienteStripe", "cust_stripe1" }, { "marca", "Visa" }, { "mesExpiracion", 12 },
                    { "anioExpiracion", 2025 }, { "ultimos4", "1234" },
                    { "fechaRegistro", DateTime.Now.ToUniversalTime() }, { "predeterminado", true }
                }
            };

            var cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(true).ReturnsAsync(false);
            cursorMock.Setup(c => c.Current).Returns(invalidBsonDoc);
            MockPagoCollection.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                    default))
                .ReturnsAsync(cursorMock.Object);

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<MPagoRepositoryException>(() => Repository.GetTodosMPago());
            Assert.IsType<IdMPagoInvalidoException>(ex.InnerException);
        }
        #endregion

    }
}
