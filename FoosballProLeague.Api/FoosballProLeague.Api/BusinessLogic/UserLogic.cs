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
    public bool CreateUser(UserRegistrationModel userRegistrationModel)
    {
        if (AccountHasValues(userRegistrationModel))
        {
            UserRegistrationModel newUserWithHashedPassword = new UserRegistrationModel
            {
                FirstName = userRegistrationModel.FirstName,
                LastName = userRegistrationModel.LastName,
                Email = userRegistrationModel.Email,
                Password = bc.HashPassword(userRegistrationModel.Password)
            };
            return _userDatabaseAccessor.CreateUser(newUserWithHashedPassword);
        }
        return false;
    }
    
    private bool AccountHasValues(UserRegistrationModel newUser)
    {
        return !string.IsNullOrEmpty(newUser.FirstName) && !string.IsNullOrEmpty(newUser.LastName) && !string.IsNullOrEmpty(newUser.Email) && !string.IsNullOrEmpty(newUser.Password);
    }
    
    //method to login user
    public bool LoginUser(string email, string password)
    {
        UserModel user = _userDatabaseAccessor.GetUser(email);
        if (user != null)
        {
            return bc.Verify(password, user.Password);
        }

        return false;
    }
    

    public List<UserModel> GetUsers()
    {
        return _userDatabaseAccessor.GetUsers();
    }
}