using FoosballProLeague.Api.Models;
using FoosballProLeague.Api.Models.FoosballModels;

namespace FoosballProLeague.Api.BusinessLogic.Interfaces
{
    public interface IUserLogic
    {
        public bool CreateUser(UserRegistrationModel userRegistrationModel);

        public bool LoginUser(string email, string password);
        public Task UpdateLeaderboard(string mode);
        public List<UserModel> GetSortedLeaderboard(string mode);
        public Dictionary<string, List<UserModel>> GetLeaderboards();
        public List<UserModel> GetAllUsers();
        public UserModel GetUserByEmail(string email);
        public UserModel GetUserById(int userId);
        public void UpdateTeamElo(MatchModel match);
        public List<MatchHistoryModel> GetMatchHistoryByUserId(int userId);
        public Task<bool> ResetPassword(string email, string newPassword);
        public Task SendPasswordResetEmail(string toEmail);
    }
}