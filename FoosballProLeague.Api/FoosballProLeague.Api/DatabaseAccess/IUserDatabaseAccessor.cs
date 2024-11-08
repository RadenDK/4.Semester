using FoosballProLeague.Api.Models;

namespace FoosballProLeague.Api.DatabaseAccess;

public interface IUserDatabaseAccessor
{
    public bool CreateUser(UserRegistrationModel newUserWithHashedPassword);

    public UserModel GetUser(string email);
    public List<UserModel> GetUsers();
    public UserModel GetUserById(int userId);
    public bool UpdateUserElo(int userId, int elo, bool is1v1);
}