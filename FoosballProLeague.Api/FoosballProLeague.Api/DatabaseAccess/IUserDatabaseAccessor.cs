using FoosballProLeague.Api.Models;
using FoosballProLeague.Api.Models.FoosballModels;

namespace FoosballProLeague.Api.DatabaseAccess;

public interface IUserDatabaseAccessor
{
    public bool CreateUser(UserRegistrationModel newUserWithHashedPassword);

    public UserModel GetUser(string email);
    public List<UserModel> GetUsers();
    public UserModel GetUserById(int userId);
    public bool UpdateUserElo(int userId, int elo, bool is1v1);
    public List<TeamModel> GetTeamsByUserId(int userId);
    public List<MatchHistoryModel> GetMatchIdByTeamId(int teamId);
}