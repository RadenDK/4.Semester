using FoosballProLeague.Api.Models;

namespace FoosballProLeague.Api.DatabaseAccess;

public interface IUserDatabaseAccessor
{
    public bool CreateUser(UserRegistrationModel newUserWithHashedPassword);

    public UserModel GetUser(string email);
    public List<UserModel> GetUsers();
}