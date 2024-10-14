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
        
        string query = "INSERT INTO Users (FirstName, LastName, Email, Password)" + 
                       "  VALUES (@FirstName, @LastName, @Email, @Password)";

        //using (SqlConnection connection = new SqlConnection(_connectionString))
        //{
          //  connection.open();
            //var rowsAffected = connection.Excecute(query, newUser);
            //userInserted = rowsAffected == 1;
        //}

        return userInserted;
    }

    
    public UserModel GetUser(string email)
    {
        UserModel user = null;

        string query = "SELECT * FROM Users WHERE Email = @Email";

        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            user = connection.Query<UserModel>(query, new { Email = email }).FirstOrDefault();
        }

        return user;
    }
}