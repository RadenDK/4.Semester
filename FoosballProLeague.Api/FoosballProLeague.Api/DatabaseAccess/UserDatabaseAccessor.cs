using FoosballProLeague.Api.Models;
using Npgsql;
using Dapper;
using System.Data;

namespace FoosballProLeague.Api.DatabaseAccess;

public class UserDatabaseAccessor : IUserDatabaseAccessor
{
    private readonly string _connectionString;
    
    public UserDatabaseAccessor(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    
    public bool CreateUser(UserRegistrationModel newUserWithHashedPassword)
    {
        bool userInserted = false;
        
        string query = "INSERT INTO users (first_name, last_name, email, password)" + 
                       "VALUES (@FirstName, @LastName, @Email, @Password)";

        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            int rowsAffected = connection.Execute(query, newUserWithHashedPassword);
            userInserted = rowsAffected == 1;
        }
        return userInserted;
    }


    public UserModel GetUser(string email)
    {
        UserModel user = null;

        string query = "SELECT * FROM Users WHERE Email = @Email";

        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            user = connection.QuerySingleOrDefault<UserModel>(query, new { Email = email });
        }
        return user;
    }

    public List<UserModel> GetUsers()
    {
        List<UserModel> users = new List<UserModel>();
        string query = "SELECT id, first_name, last_name, email, department_id, company_id, elo_1v1, elo_2v2 " +
                       "FROM users";

        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            users = connection.Query<UserModel>(query).ToList();
        }
        return users;
    }
}