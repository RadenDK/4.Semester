using FoosballProLeague.Api.Models;

namespace FoosballProLeague.Api.DatabaseAccess.Interfaces
{
    public interface IUserDatabaseAccessor
    {
        public UserModel GetUserByEmail(string email);
        public List<UserModel> GetAllUsers();
        public UserModel GetUserById(int userId);
        public bool CreateUser(UserRegistrationModel newUserWithHashedPassword);
        public bool UpdateUserElo(int userId, int elo, bool is1v1);
        public List<MatchHistoryModel> GetMatchHistoryByUserId(int userId);
        public bool UpdatePasswordByEmail(string email, string password);
    }
}
