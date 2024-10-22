using FoosballProLeague.Api.Models;

namespace FoosballProLeague.Api.BusinessLogic;

public interface IUserLogic
{
    public bool CreateUser(UserRegistrationModel userRegistrationModel);
    
    public bool LoginUser(string email, string password);
    public List<UserModel> GetUsers();
}