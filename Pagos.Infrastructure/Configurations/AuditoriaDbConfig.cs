using Pagos.Domain.Exceptions;
using MongoDB.Driver;

namespace Pagos.Infrastructure.Configurations
{
    public class AuditoriaDbConfig
    {
        public MongoClient client;
        public IMongoDatabase db;

        public AuditoriaDbConfig()
        {
            try
            {
                string connectionUri = Environment.GetEnvironmentVariable("MONGODB_CNN");

                if (string.IsNullOrWhiteSpace(connectionUri))
                {
                    throw new ConexionBdInvalida();
                }

                var settings = MongoClientSettings.FromConnectionString(connectionUri);
                settings.ServerApi = new ServerApi(ServerApiVersion.V1);

                client = new MongoClient(settings);

                string databaseName = Environment.GetEnvironmentVariable("MONGODB_NAME_AUDITORIAS");
                if (string.IsNullOrWhiteSpace(databaseName))
                {
                    throw new NombreBdInvalido();
                }

                db = client.GetDatabase(databaseName);
            }
            catch (MongoException ex)
            {
                throw new MongoDBConnectionException(ex);
            }
            catch (Exception ex)
            {
                throw new MongoDBUnnexpectedException(ex);
            }
        }
    }
}
