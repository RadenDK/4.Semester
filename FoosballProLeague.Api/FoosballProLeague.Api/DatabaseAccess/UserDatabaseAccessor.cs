using FoosballProLeague.Api.Models;
using Npgsql;
using Dapper;
using System.Data;
using Microsoft.AspNetCore.SignalR;

namespace FoosballProLeague.Api.DatabaseAccess;

public class UserDatabaseAccessor : IUserDatabaseAccessor
{
    private readonly string _connectionString;
    
    public UserDatabaseAccessor(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DatabaseConnection");
    }
    
    // method to create user for registration
    public bool CreateUser(UserRegistrationModel newUserWithHashedPassword)
    {
        bool userInserted = false;
        
        string query = "INSERT INTO users (first_name, last_name, email, password, department_Id, company_Id, elo_1v1, elo_2v2)" + 
                       "VALUES (@FirstName, @LastName, @Email, @Password, @DepartmentId, @CompanyId, @Elo1v1, @Elo2v2)";

        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            int rowsAffected = connection.Execute(query, newUserWithHashedPassword);
            userInserted = rowsAffected == 1;
        }
        return userInserted;
    }

    // method to login user / method to get user by email
    public UserModel GetUser(string email)
    {
        UserModel user = null;

        string query = "SELECT email AS Email, password AS Password FROM Users WHERE Email = @Email";

        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            user = connection.QuerySingleOrDefault<UserModel>(query, new { Email = email });
        }
        return user;
    }

    public UserModel GetUserById(int userId)
    {
        UserModel user = null;

        string query = "SELECT email AS Email, password AS Password FROM Users WHERE id = @userId";
        
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            user = connection.QuerySingleOrDefault<UserModel>(query, new { userId = userId });
        }
        return user;
    }

    public List<UserModel> GetUsers()
    {
        List<UserModel> users = new List<UserModel>();
        string query = "SELECT id AS Id, first_name AS FirstName, last_name AS LastName, email AS Email, elo_1v1 AS Elo1v1, elo_2v2 AS Elo2v2 " +
                       "FROM users";

        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            users = connection.Query<UserModel>(query).ToList();
        }
        return users;
    }

    public bool UpdateUserElo(int userId, int elo, bool is1v1)
    {
        bool rowsAffected = false;
        string query;
        if (is1v1)
        {
            query = "UPDATE users SET elo_1v1 = @elo WHERE id = @UserId";
        }
        else
        {
            query = "UPDATE users SET elo_2v2 = @elo WHERE id = @UserId";
        }

        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            int affectedRows = connection.Execute(query, new { UserId = userId, elo });
            rowsAffected = affectedRows > 0;
        }

        return rowsAffected;
    }
    
}