using FoosballProLeague.Api.Models;

namespace FoosballProLeague.Api.DatabaseAccess;

public interface IUserDatabaseAccessor
{
    public bool CreateUser(UserRegistrationModel userRegistrationModel);
}