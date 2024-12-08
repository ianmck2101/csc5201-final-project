using Fetch.Models.Data;
using Npgsql;

namespace Fetch.Consumer
{
    public interface IConsumerDAL
    {
        void EnsureTablesExist();

        Task<IEnumerable<Provider>> LoadAllProviders();
        Task<IEnumerable<ProviderRequestAssociation>> LoadRequestsForProvider(int providerId);
        Task AddProviderRequestAssociation(ProviderRequestAssociation association);
        Task UpdateRequestStatus(int requestId, Status status);
    }

    public class ConsumerDAL : IConsumerDAL
    {
        private readonly string _connectionString;

        public ConsumerDAL()
        {
            _connectionString = "Host=database;Database=fetchdb;Username=fetchuser;Password=fetchpassword";
        }

        public void EnsureTablesExist()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            // Ensure Providers table exists
            var providersTableQuery = @"
                CREATE TABLE IF NOT EXISTS providers (
                    id SERIAL PRIMARY KEY,
                    name VARCHAR(255) NOT NULL,
                    email VARCHAR(255) NOT NULL,
                    provider_categories INTEGER[] NOT NULL
                );";

            // Ensure ProviderRequestAssociations table exists
            var providerRequestAssociationsTableQuery = @"
                CREATE TABLE IF NOT EXISTS provider_request_associations (
                    id SERIAL PRIMARY KEY,
                    description TEXT NOT NULL,
                    status VARCHAR(50) NOT NULL DEFAULT 'open',
                    provider_id INT REFERENCES providers(id) ON DELETE CASCADE
                );";

            using var providersCommand = new NpgsqlCommand(providersTableQuery, connection);
            providersCommand.ExecuteNonQuery();

            using var providerRequestAssociationsCommand = new NpgsqlCommand(providerRequestAssociationsTableQuery, connection);
            providerRequestAssociationsCommand.ExecuteNonQuery();
        }

        public async Task<IEnumerable<Provider>> LoadAllProviders()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT id, name, email, provider_categories FROM providers";
            using var command = new NpgsqlCommand(query, connection);

            using var reader = await command.ExecuteReaderAsync();
            var providers = new List<Provider>();

            while (await reader.ReadAsync())
            {
                providers.Add(new Provider()
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2),
                    Categories = reader.GetFieldValue<ServiceCategories[]>(3)
                });
            }

            return providers ?? Enumerable.Empty<Provider>();
        }

        public async Task<IEnumerable<ProviderRequestAssociation>> LoadRequestsForProvider(int providerId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT id, description, status, provider_id 
                FROM provider_request_associations 
                WHERE provider_id = @ProviderId";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("ProviderId", providerId);

            using var reader = await command.ExecuteReaderAsync();
            var associations = new List<ProviderRequestAssociation>();

            while (await reader.ReadAsync())
            {
                associations.Add(new ProviderRequestAssociation()
                {
                    Id = reader.GetInt32(0),
                    Description = reader.GetString(1),
                    Status = reader.GetFieldValue<Status>(2),
                    ProviderId = reader.GetInt32(3)
                });
            }

            return associations ?? Enumerable.Empty<ProviderRequestAssociation>();
        }

        public async Task AddProviderRequestAssociation(ProviderRequestAssociation association)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                INSERT INTO provider_request_associations (description, status, provider_id)
                VALUES (@Description, @Status, @ProviderId)";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("Description", association.Description);
            command.Parameters.AddWithValue("Status", (byte)association.Status);
            command.Parameters.AddWithValue("ProviderId", association.ProviderId);

            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateRequestStatus(int requestId, Status status)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "UPDATE provider_request_associations SET status = @Status WHERE id = @RequestId";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("Status", (byte)status);
            command.Parameters.AddWithValue("RequestId", requestId);

            await command.ExecuteNonQueryAsync();
        }
    }
}
