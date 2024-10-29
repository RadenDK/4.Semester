using FoosballProLeague.Api.Models;
using Npgsql;
using Dapper;
using System.Data;

namespace FoosballProLeague.Api.DatabaseAccess;

public class UserDatabaseAccessor : DatabaseAccessor, IUserDatabaseAccessor
{
    
    public UserDatabaseAccessor(IConfiguration configuration) : base(configuration)
    {
    }
    
    // method to create user for registration
    public bool CreateUser(UserRegistrationModel newUserWithHashedPassword)
    {
        bool userInserted = false;
        
        string query = @"INSERT INTO users (first_name, last_name, email, password, department_Id, company_Id, elo_1v1, elo_2v2)
                        VALUES (@FirstName, @LastName, @Email, @Password, @DepartmentId, @CompanyId, @Elo1v1, @Elo2v2)";

        using (IDbConnection connection = GetConnection())
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

        string query = "SELECT email, password FROM Users WHERE email = @Email";

        using (IDbConnection connection = GetConnection())
        {
            connection.Open();
            user = connection.QuerySingleOrDefault<UserModel>(query, new { Email = email });
        }
        return user;
    }

    public List<UserModel> GetUsers()
    {
        List<UserModel> users = new List<UserModel>();
        string query = "SELECT id, first_name, last_name, email, elo_1v1, elo_2v2 FROM users";

        using (IDbConnection connection = GetConnection())
        {
            connection.Open();
            users = connection.Query<UserModel>(query).ToList();
        }
        return users;
    }
}