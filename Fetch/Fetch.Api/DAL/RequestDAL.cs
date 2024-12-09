using Npgsql;
using Fetch.Models.Data;

namespace Fetch.Api.Data
{
    public interface IRequestDAL
    {
        public void EnsureTablesExist();
        public int CreateNewRequest(BaseRequest request);
        public bool DeleteRequest(int id);
        public BaseRequest? GetRequest(int id);
        public IEnumerable<BaseRequest> GetAllRequests();
        public IEnumerable<ProviderRequestAssociation> LoadAssociatonsByProviderId(int providerId);
    }

    public class RequestDAL : IRequestDAL
    {
        private readonly string _connectionString;

        public RequestDAL()
        {
            _connectionString = "Host=database;Database=fetchdb;Username=fetchuser;Password=fetchpassword";
        }

        public void EnsureTablesExist()
        {
            Thread.Sleep(3000);

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            // Check if the table exists
            var checkTableQuery = @"
                CREATE TABLE IF NOT EXISTS requests (
                    id SERIAL PRIMARY KEY,
                    title VARCHAR(255) NOT NULL,
                    description TEXT NOT NULL,
                    price DECIMAL NOT NULL,
                    due_date TIMESTAMP NOT NULL, 
                    category INT NOT NULL
                );";

            using var command = new NpgsqlCommand(checkTableQuery, connection);
            command.ExecuteNonQuery();

            // Ensure Providers table exists
            var providersTableQuery = @"
                CREATE TABLE IF NOT EXISTS providers (
                    id SERIAL PRIMARY KEY,
                    name VARCHAR(255) NOT NULL,
                    email VARCHAR(255) NOT NULL,
                    provider_categories INTEGER[] NOT NULL,
                    provider_contacts INTEGER[] NOT NULL
                );";

            // Ensure ProviderRequestAssociations table exists
            var providerRequestAssociationsTableQuery = @"
                CREATE TABLE IF NOT EXISTS provider_request_associations (
                    id SERIAL PRIMARY KEY,
                    title TEXT NOT NULL, 
                    description TEXT NOT NULL,
                    status INT NOT NULL,
                    request_id INT NOT NULL,
                    provider_id INT REFERENCES providers(id) ON DELETE CASCADE
                );";

            checkTableQuery = @"
                CREATE TABLE IF NOT EXISTS users (
                    id SERIAL PRIMARY KEY,
                    username VARCHAR(255) NOT NULL,
                    password TEXT NOT NULL, 
                    role INT NOT NULL
                );";

            using var userCommand = new NpgsqlCommand(checkTableQuery, connection);
            userCommand.ExecuteNonQuery();

            var addTestUsers = @"
                INSERT INTO users (username, password, role) VALUES
                ('testrequestor', 'testpass', 0),
                ('testproviderA', 'testpassA', 1),
                ('testproviderB', 'testpassB', 1),
                ('testproviderC', 'testpassB', 1);";

            using var insertCommand = new NpgsqlCommand(addTestUsers, connection);
            insertCommand.ExecuteNonQuery();

            using var providersCommand = new NpgsqlCommand(providersTableQuery, connection);
            providersCommand.ExecuteNonQuery();

            using var providerRequestAssociationsCommand = new NpgsqlCommand(providerRequestAssociationsTableQuery, connection);
            providerRequestAssociationsCommand.ExecuteNonQuery();

            var providerContacts = LoadProviderContacts(connection);

            var insertProvidersQuery = $@"
                INSERT INTO providers (name, email, provider_categories, provider_contacts)
                VALUES
                    ('ProviderA', 'providera@example.com', ARRAY[0], ARRAY[{providerContacts.Single(x => x.Username.Equals("testproviderA")).Id}]),
                    ('ProviderB', 'providerb@example.com', ARRAY[0, 1], ARRAY[{providerContacts.Single(x => x.Username.Equals("testproviderB")).Id}]),
                    ('ProviderC', 'providerc@example.com', ARRAY[1], ARRAY[{providerContacts.Single(x => x.Username.Equals("testproviderC")).Id}])
                ON CONFLICT DO NOTHING;";
            using var insertProviderCommand = new NpgsqlCommand(insertProvidersQuery, connection);
            insertProviderCommand.ExecuteNonQuery();
        }

        private IEnumerable<User> LoadProviderContacts(NpgsqlConnection connection)
        {
            var script = @"
                SELECT * FROM users where role = 1;";

            using var command = new NpgsqlCommand(script, connection);

            using var reader = command.ExecuteReader();

            var results = new List<User>();
            while (reader.Read())
            {
                results.Add(new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    Role = (Role)reader.GetInt32(3)
                });
            }

            return results;
        }

        public int CreateNewRequest(BaseRequest request)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var query = "INSERT INTO requests (title, description, price, due_date, category) VALUES (@title, @description, @price, @due_date, @category) RETURNING id;";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@title", request.Title);
            command.Parameters.AddWithValue("@description", request.Description);
            command.Parameters.AddWithValue("@price", request.Price);
            command.Parameters.AddWithValue("@due_date", request.DueDate);
            command.Parameters.AddWithValue("@category", (byte)request.Category);

#pragma warning disable CS8605 // Unboxing a possibly null value.
            var newRequestId = (int)command.ExecuteScalar();
#pragma warning restore CS8605 // Unboxing a possibly null value.
            return newRequestId;
        }


        public bool DeleteRequest(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var query = "DELETE FROM requests WHERE id = @id";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);

            var rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        public BaseRequest? GetRequest(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var query = "SELECT id, title, description, price, due_date, category FROM requests WHERE id = @id";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new BaseRequest
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.GetString(2),
                    Price = reader.GetDecimal(3),
                    DueDate = (DateTimeOffset)reader.GetDateTime(4),
                    Category = (ServiceCategories)reader.GetInt32(5)
                };
            }

            return null;
        }

        public IEnumerable<BaseRequest> GetAllRequests()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var query = "SELECT id, title, description, price, due_date, category FROM requests";
            using var command = new NpgsqlCommand(query, connection);

            using var reader = command.ExecuteReader();
            var requests = new List<BaseRequest>();

            while (reader.Read())
            {
                requests.Add(new BaseRequest
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.GetString(2),
                    Price = reader.GetDecimal(3),
                    DueDate = (DateTimeOffset)reader.GetDateTime(4),
                    Category = (ServiceCategories)reader.GetInt32(5)
                });
            }

            return requests;
        }

        public IEnumerable<ProviderRequestAssociation> LoadAssociatonsByProviderId(int providerId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var query = @"
                SELECT id, title, description, status, request_id, provider_id 
                FROM provider_request_associations 
                WHERE provider_id = @ProviderId";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("ProviderId", providerId);

            using var reader = command.ExecuteReader();
            var associations = new List<ProviderRequestAssociation>();

            while (reader.Read())
            {
                associations.Add(new ProviderRequestAssociation()
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.GetString(2),
                    Status = (Status)reader.GetInt32(3),
                    RequestId = reader.GetInt32(4),
                    ProviderId = reader.GetInt32(5)
                });
            }

            return associations ?? Enumerable.Empty<ProviderRequestAssociation>();
        }
    }
}
