using FoosballProLeague.Api.Models;
using Npgsql;

namespace FoosballProLeague.Api.DatabaseAccess;

public class UserDatabaseAccessor
{
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
}