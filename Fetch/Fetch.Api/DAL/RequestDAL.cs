using Npgsql;
using Fetch.Models.Data;

namespace Fetch.Api.Data
{
    public interface IRequestDAL
    {
        public void EnsureTablesExist();
        public void CreateNewRequest(BaseRequest request);
        public bool DeleteRequest(int id);
        public BaseRequest? GetRequest(int id);
        public IEnumerable<BaseRequest> GetAllRequests();
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
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            // Check if the table exists
            var checkTableQuery = @"
                CREATE TABLE IF NOT EXISTS requests (
                    id SERIAL PRIMARY KEY,
                    title VARCHAR(255) NOT NULL,
                    description TEXT NOT NULL,
                    price DECIMAL NOT NULL,
                    due_date TIMESTAMP NOT NULL
                );";

            using var command = new NpgsqlCommand(checkTableQuery, connection);
            command.ExecuteNonQuery();
        }

        public void CreateNewRequest(BaseRequest request)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var query = "INSERT INTO requests (title, description, price, due_date) VALUES (@title, @description, @price, @due_date)";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@title", request.Title);
            command.Parameters.AddWithValue("@description", request.Description);
            command.Parameters.AddWithValue("@price", request.Price);
            command.Parameters.AddWithValue("@due_date", request.DueDate);

            command.ExecuteNonQuery();
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

            var query = "SELECT id, title, description, price, due_date FROM requests WHERE id = @id";
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
                    DueDate = (DateTimeOffset)reader.GetDateTime(4)
                };
            }

            return null;
        }

        public IEnumerable<BaseRequest> GetAllRequests()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var query = "SELECT id, title, description, price, due_date FROM requests";
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
                    DueDate = (DateTimeOffset)reader.GetDateTime(4)
                });
            }

            return requests;
        }
    }
}
