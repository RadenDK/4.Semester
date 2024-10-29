using FoosballProLeague.Api.Models;
using FoosballProLeague.Api.Models.FoosballModels;

namespace FoosballProLeague.Api.BusinessLogic;

public interface IUserLogic
{
    public bool CreateUser(UserRegistrationModel userRegistrationModel);
    
    public bool LoginUser(string email, string password);
    public List<UserModel> GetUsers();
    public UserModel GetUser(string email);
    public UserModel GetUserById(int userId);
    public bool UpdateUserElo(int userId,int elo, bool is1v1);
    public int CalculateNewElo(int userElo, int opponentElo, bool won);
    public void UpdateTeamElo(TeamModel redTeam, TeamModel blueTeam, bool redTeamWon, bool is1v1);
}