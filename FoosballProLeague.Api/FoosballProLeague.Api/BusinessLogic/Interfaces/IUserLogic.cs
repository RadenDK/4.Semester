using FoosballProLeague.Api.Models;
using FoosballProLeague.Api.Models.FoosballModels;

namespace FoosballProLeague.Api.BusinessLogic.Interfaces
{
    public interface IUserLogic
    {
        public bool CreateUser(UserRegistrationModel userRegistrationModel);

        public bool LoginUser(string email, string password);
        public List<UserModel> GetAllUsers();
        public UserModel GetUserByEmail(string email);
        public UserModel GetUserById(int userId);
        public void UpdateTeamElo(MatchModel match);
        public List<MatchHistoryModel> GetMatchHistoryByUserId(int userId);
    }
}