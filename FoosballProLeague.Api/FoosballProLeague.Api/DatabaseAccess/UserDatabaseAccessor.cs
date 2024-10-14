using FoosballProLeague.Api.Models;
using Npgsql;
using Dapper;
using System.Data;
using System.Data.SqlTypes;

namespace FoosballProLeague.Api.DatabaseAccess;

public class UserDatabaseAccessor
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