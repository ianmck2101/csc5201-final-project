using Fetch.Models.Data;
using Fetch.Models.Events;
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
        Task ProcessAcceptedRequest(RequestUpdated updateRequest);
        Task ProcessClosedRequest(RequestUpdated updateRequest);
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
                    title TEXT NOT NULL, 
                    description TEXT NOT NULL,
                    status VARCHAR(50) NOT NULL DEFAULT 'open',
                    request_id INT NOT NULL,
                    provider_id INT REFERENCES providers(id) ON DELETE CASCADE
                );";

            using var providersCommand = new NpgsqlCommand(providersTableQuery, connection);
            providersCommand.ExecuteNonQuery();

            using var providerRequestAssociationsCommand = new NpgsqlCommand(providerRequestAssociationsTableQuery, connection);
            providerRequestAssociationsCommand.ExecuteNonQuery();

            var insertProvidersQuery = @"
                INSERT INTO providers (name, email, provider_categories)
                VALUES
                    ('ProviderA', 'providera@example.com', ARRAY[0]),
                    ('ProviderB', 'providerb@example.com', ARRAY[0, 1]),
                    ('ProviderC', 'providerc@example.com', ARRAY[1])
                ON CONFLICT DO NOTHING;";
            using var insertCommand = new NpgsqlCommand(insertProvidersQuery, connection);
            insertCommand.ExecuteNonQuery();
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
                var categoriesArray = reader.GetFieldValue<int[]>(3);
                var categories = categoriesArray.Select(c => (ServiceCategories)c).ToArray();

                providers.Add(new Provider()
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2),
                    Categories = categories
                });
            }

            return providers ?? Enumerable.Empty<Provider>();
        }

        public async Task<IEnumerable<ProviderRequestAssociation>> LoadRequestsForProvider(int providerId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT id, title, description, status, request_id, provider_id 
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
                    Title = reader.GetString(1),
                    Description = reader.GetString(2),
                    Status = reader.GetFieldValue<Status>(3),
                    RequestId = reader.GetInt32(4),
                    ProviderId = reader.GetInt32(5)
                });
            }

            return associations ?? Enumerable.Empty<ProviderRequestAssociation>();
        }

        public async Task AddProviderRequestAssociation(ProviderRequestAssociation association)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                INSERT INTO provider_request_associations (title, description, status, request_id, provider_id)
                VALUES (@Title, @Description, @Status, @RequestId, @ProviderId)";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("Title", association.Title);
            command.Parameters.AddWithValue("Description", association.Description);
            command.Parameters.AddWithValue("Status", (byte)association.Status);
            command.Parameters.AddWithValue("RequestId", association.RequestId);
            command.Parameters.AddWithValue("ProviderId", association.ProviderId);

            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateRequestStatus(int associationId, Status status)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "UPDATE provider_request_associations SET status = @Status WHERE id = @AssociationId";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("Status", (byte)status);
            command.Parameters.AddWithValue("AssociationId", associationId);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<ProviderRequestAssociation>> LoadAssociationsByRequestId(int requestId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT id, title, description, status, request_id, provider_id 
                FROM provider_request_associations 
                WHERE request_id = @RequestId";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("RequestId", requestId);

            using var reader = await command.ExecuteReaderAsync();
            var associations = new List<ProviderRequestAssociation>();

            while (await reader.ReadAsync())
            {
                associations.Add(new ProviderRequestAssociation()
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.GetString(2),
                    Status = reader.GetFieldValue<Status>(3),
                    RequestId = reader.GetInt32(4),
                    ProviderId = reader.GetInt32(5)
                });
            }

            return associations ?? Enumerable.Empty<ProviderRequestAssociation>();
        }

        public async Task ProcessAcceptedRequest(RequestUpdated updateRequest)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var acceptQuery = @"
                    UPDATE provider_request_associations 
                    SET status = @Status 
                    WHERE request_id = @RequestId AND provider_id = @ProviderId";
                using var acceptCommand = new NpgsqlCommand(acceptQuery, connection);
                acceptCommand.Parameters.AddWithValue("Status", (byte)Status.Accepted);
                acceptCommand.Parameters.AddWithValue("RequestId", updateRequest.RequestId);
#pragma warning disable CS8604 // Possible null reference argument.
                acceptCommand.Parameters.AddWithValue("ProviderId", updateRequest.ProviderId);
#pragma warning restore CS8604 // Possible null reference argument.

                await acceptCommand.ExecuteNonQueryAsync();

                var closeQuery = @"
                    UPDATE provider_request_associations 
                    SET status = @Status 
                    WHERE request_id = @RequestId AND provider_id != @ProviderId";
                using var closeCommand = new NpgsqlCommand(closeQuery, connection);
                closeCommand.Parameters.AddWithValue("Status", (byte)Status.Closed);
                closeCommand.Parameters.AddWithValue("RequestId", updateRequest.RequestId);
                closeCommand.Parameters.AddWithValue("ProviderId", updateRequest.ProviderId);

                await closeCommand.ExecuteNonQueryAsync();

                // Commit the transaction
                await transaction.CommitAsync();

                Console.WriteLine($"Request {updateRequest.RequestId} successfully accepted by provider {updateRequest.ProviderId}.");
            }
            catch (Exception ex)
            {
                // Rollback the transaction in case of an error
                await transaction.RollbackAsync();
                Console.WriteLine($"Error while processing accepted request: {ex.Message}");
            }
        }

        public async Task ProcessClosedRequest(RequestUpdated updateRequest)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Update all associations for the given request to "Closed"
            var closeQuery = @"
                UPDATE provider_request_associations 
                SET status = @Status 
                WHERE request_id = @RequestId";
            using var closeCommand = new NpgsqlCommand(closeQuery, connection);
            closeCommand.Parameters.AddWithValue("Status", (byte)Status.Closed);
            closeCommand.Parameters.AddWithValue("RequestId", updateRequest.RequestId);

            await closeCommand.ExecuteNonQueryAsync();

            Console.WriteLine($"Request {updateRequest.RequestId} successfully closed.");
        }
    }
}
