using System;
using System.Threading.Tasks;
using Npgsql;

namespace OpenTelemetryTest
{
    public class UserRepository
    {
        private const string ConnectionString = "Server=185.229.224.209;Username=selectel;Database=selectel;Port=5432;Password=selectel";

        public async Task Clear()
        {
            await using var connection = new NpgsqlConnection(ConnectionString);
            
            connection.Open();

            await using var command = new NpgsqlCommand("DROP TABLE IF EXISTS users", connection);

            command.ExecuteNonQuery();

            await using var command2 = new NpgsqlCommand("CREATE TABLE users(id serial PRIMARY KEY, name VARCHAR(50), money INTEGER)", connection);

            await command2.ExecuteNonQueryAsync();
        }
        
        public async Task Add(User user)
        {
            await using var connection = new NpgsqlConnection(ConnectionString);
            
            connection.Open();

            await using var command = new NpgsqlCommand("INSERT INTO users (name, money) VALUES (@n1, @m1)", connection);

            command.Parameters.AddWithValue("n1", user.Name);
            command.Parameters.AddWithValue("m1", user.Money);
            
            await command.ExecuteNonQueryAsync();
        }

        public async Task<User> Get(int id)
        {
            await using var connection = new NpgsqlConnection(ConnectionString);
            
            connection.Open();

            await using var command = new NpgsqlCommand("SELECT id, name, money FROM users", connection);

            Console.WriteLine();

            var reader = await command.ExecuteReaderAsync();

            reader.Read();

            var name = reader.GetString(1);
            var money = reader.GetInt32(2);
            
            await reader.CloseAsync();
            
            return new User() {Name=name, Money = money};
        }
    }
}