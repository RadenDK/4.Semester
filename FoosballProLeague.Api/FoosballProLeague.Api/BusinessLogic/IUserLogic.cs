using FoosballProLeague.Api.Models;

namespace FoosballProLeague.Api.BusinessLogic;

public interface IUserLogic
{
    public bool CreateUser(UserRegistrationModel userRegistrationModel);
    public List<UserModel> GetUsers();
}