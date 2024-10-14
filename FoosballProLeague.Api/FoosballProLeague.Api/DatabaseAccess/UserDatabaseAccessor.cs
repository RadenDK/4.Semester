using FoosballProLeague.Api.Models;
using Npgsql;
using Dapper;

namespace FoosballProLeague.Api.DatabaseAccess;

public class UserDatabaseAccessor : IUserDatabaseAccessor
{
    private readonly string _connectionString;
    
    public UserDatabaseAccessor(string connectionString)
    {
        _connectionString = connectionString;
    }
    public bool CreateUser(UserRegistrationModel newUser)
    {
        bool userInserted = false;
        
        string query = "INSERT INTO users (first_name, last_name, email, password)" + 
                       "VALUES (@FirstName, @LastName, @Email, @Password)";

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            int rowsAffected = connection.Execute(query, newUser);
            userInserted = rowsAffected == 1;
        }
        return userInserted;
    }
}