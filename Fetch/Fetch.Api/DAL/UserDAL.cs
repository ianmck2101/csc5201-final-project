using Fetch.Models.Data;
using Npgsql;

namespace Fetch.Api.DAL
{
    public interface IUserDAL
    {
        public User? Authenticate(string username, string password);
        IEnumerable<User> LoadAllUsers();
        User? LoadUserByUsername(string username);
        Provider? LoadProviderForUser(User user);
    }

    public class UserDAL : IUserDAL
    {
        private readonly string _connectionString;

        public UserDAL()
        {
            _connectionString = "Host=database;Database=fetchdb;Username=fetchuser;Password=fetchpassword";
        }

        public User? Authenticate(string username, string password)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var query = "SELECT id, username, password, role from users where username = @username AND password = @password";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", password);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    Role = (Role)reader.GetInt32(3)
                };
            }

            return null;
        }

        public IEnumerable<User> LoadAllUsers()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var query = "SELECT * from users;";
            using var command = new NpgsqlCommand(query, connection);

            var results = new List<User>();
            using var reader = command.ExecuteReader();

            while (reader.Read()) 
            {
                results.Add(new User()
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    Role = (Role)reader.GetInt32(3)
                });
            }

            return results;
        }

        public Provider? LoadProviderForUser(User user)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var query = @"
                SELECT *
                FROM providers
                WHERE @Id = ANY(provider_contacts)";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", user.Id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                var categoriesArray = reader.GetFieldValue<int[]>(3);
                var categories = categoriesArray.Select(c => (ServiceCategories)c).ToArray();

                var providerContacts = reader.GetFieldValue<int[]>(4);

                return new Provider()
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2),
                    Categories = categories,
                    ProviderContacts = providerContacts
                };
            }

            return null;
        }


        public User? LoadUserByUsername(string username)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var query = @"
                SELECT *
                FROM users
                WHERE username = @Username";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new User()
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    Role = (Role)reader.GetInt32(3)
                };
            }

            return null; // Return null if no provider found
        }
    }
}