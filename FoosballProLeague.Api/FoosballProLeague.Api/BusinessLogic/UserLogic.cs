using FoosballProLeague.Api.Models;
using FoosballProLeague.Api.DatabaseAccess;
using bc = BCrypt.Net.BCrypt;

namespace FoosballProLeague.Api.BusinessLogic;

public class UserLogic : IUserLogic
{
    IUserDatabaseAccessor _userDatabaseAccessor;
    
    public UserLogic(IUserDatabaseAccessor userDatabaseAccessor)
    {
        _userDatabaseAccessor = userDatabaseAccessor;
    }
    
    // method to create user for registration
    public bool CreateUser(UserRegistrationModel userRegistrationModel)
    {
        if (AccountHasValues(userRegistrationModel))
        {
            UserRegistrationModel newUserWithHashedPassword = new UserRegistrationModel
            {
                FirstName = userRegistrationModel.FirstName,
                LastName = userRegistrationModel.LastName,
                Email = userRegistrationModel.Email,
                Password = bc.HashPassword(userRegistrationModel.Password),
                DepartmentId = userRegistrationModel.DepartmentId,
                CompanyId = userRegistrationModel.CompanyId,
                Elo1v1 = 500,
                Elo2v2 = 500
            };
            return _userDatabaseAccessor.CreateUser(newUserWithHashedPassword);
        }
        return false;
    }
    
    // checks if the account has values
    private bool AccountHasValues(UserRegistrationModel newUser)
    {
        if (newUser == null)
        {
            return false;
        }
        
        if (string.IsNullOrEmpty(newUser.FirstName))
        {
            return false;
        }

        if (string.IsNullOrEmpty(newUser.LastName))
        {
            return false;
        }

        if (string.IsNullOrEmpty(newUser.Email) && _userDatabaseAccessor.GetUser(newUser.Email) != null)
        {
            return false;
        }

        if (string.IsNullOrEmpty(newUser.Password))
        {
            return false;
        }

        return true;
    }
    
    //method to login user
    public bool LoginUser(string email, string password)
    {
        UserModel user = _userDatabaseAccessor.GetUser(email);
        if (user == null || !bc.Verify(password, user.Password))
        {
            return bc.Verify(password, user.Password);
        }
        return false;
    }
    
    // get all user in a list
    public List<UserModel> GetUsers()
    {
        return _userDatabaseAccessor.GetUsers();
    }

    public UserModel GetUser(string email)
    {
        return _userDatabaseAccessor.GetUser(email);
    }
}