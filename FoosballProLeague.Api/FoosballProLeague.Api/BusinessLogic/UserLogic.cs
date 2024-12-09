using FoosballProLeague.Api.BusinessLogic.Interfaces;
using FoosballProLeague.Api.DatabaseAccess.Interfaces;
using FoosballProLeague.Api.Models;
using FoosballProLeague.Api.Models.FoosballModels;
using bc = BCrypt.Net.BCrypt;
using Microsoft.AspNetCore.SignalR;
using FoosballProLeague.Api.Hubs;

namespace FoosballProLeague.Api.BusinessLogic
{

    public class UserLogic : IUserLogic
    {
        private readonly IUserDatabaseAccessor _userDatabaseAccessor;
        private readonly IHubContext<HomepageHub> _hubContext;

        public UserLogic(IUserDatabaseAccessor userDatabaseAccessor, IHubContext<HomepageHub> hubContext)
        {
            _userDatabaseAccessor = userDatabaseAccessor;
            _hubContext = hubContext;
        }

        // method to create user for registration
        public bool CreateUser(UserRegistrationModel userRegistrationModel)
        {
            if (AccountHasValues(userRegistrationModel))
            {
                UserRegistrationModel newUserWithHashedPassword = new UserRegistrationModel
                {
                    FirstName = userRegistrationModel.FirstName,
                    LastName = userRegistrationModel.LastName,
                    Email = userRegistrationModel.Email?.ToLower() ?? string.Empty, // Store email in lowercase, with a default value if empty
                    Password = bc.HashPassword(userRegistrationModel.Password),
                    DepartmentId = userRegistrationModel.DepartmentId,
                    CompanyId = userRegistrationModel.CompanyId,
                    Elo1v1 = 500,
                    Elo2v2 = 500
                };
                return _userDatabaseAccessor.CreateUser(newUserWithHashedPassword);
            }
            return false;
        }

        // checks if the account has values
        private bool AccountHasValues(UserRegistrationModel newUser)
        {
            if (newUser == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(newUser.FirstName) ||
                string.IsNullOrEmpty(newUser.LastName) ||
                string.IsNullOrEmpty(newUser.Email) ||
                string.IsNullOrEmpty(newUser.Password))
            {
                return false;
            }

            if (_userDatabaseAccessor.GetUserByEmail(newUser.Email) != null)
            {
                return false;
            }

            return true;
        }

        public async Task UpdateLeaderboard(string mode)
        {
            List<UserModel> leaderboard = GetSortedLeaderboard(mode);
            if (_hubContext.Clients != null)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveLeaderboardUpdate", leaderboard);
            }
        }

        public List<UserModel> GetSortedLeaderboard(string mode)
        {
            List<UserModel> users = _userDatabaseAccessor.GetAllUsers();
            if (mode == "1v1")
            {
                return users.OrderByDescending(u => u.Elo1v1).ToList();
            }
            else if (mode == "2v2")
            {
                return users.OrderByDescending(u => u.Elo2v2).ToList();
            }
            else
            {
                throw new ArgumentException("Invalid mode specified");
            }
        }

        // New method to get both 1v1 and 2v2 leaderboards
        public Dictionary<string, List<UserModel>> GetLeaderboards()
        {
            Dictionary<string, List<UserModel>> leaderboards = new Dictionary<string, List<UserModel>>
        {
            { "1v1", GetSortedLeaderboard("1v1") },
            { "2v2", GetSortedLeaderboard("2v2") }
        };
            return leaderboards;
        }

        //method to login user
        public bool LoginUser(string email, string password)
        {
            UserModel user = _userDatabaseAccessor.GetUserByEmail(email);

            if (user == null)
            {
                return false;
            }

            return bc.Verify(password, user.Password);
        }

        // get all user in a list
        public List<UserModel> GetAllUsers()
        {
            return _userDatabaseAccessor.GetAllUsers();
        }

        public UserModel GetUserByEmail(string email)
        {
            return _userDatabaseAccessor.GetUserByEmail(email);
        }

        public UserModel GetUserById(int userId)
        {
            return _userDatabaseAccessor.GetUserById(userId);
        }

        public void UpdateTeamElo(MatchModel match)
        {
            // Determine if the match is a 1v1 or 2v2 based on team composition
            bool is1v1 = match.RedTeam.User2 == null && match.BlueTeam.User2 == null;

            // Update ELO ratings for the Red Team based on the average ELO of the opposing Blue Team
            UpdateTeamEloForPlayers(match.RedTeam, match.BlueTeam.GetTeamEloAverage(), match.TeamRedScore == 10, is1v1);

            // Update ELO ratings for the Blue Team based on the average ELO of the opposing Red Team
            UpdateTeamEloForPlayers(match.BlueTeam, match.RedTeam.GetTeamEloAverage(), match.TeamBlueScore == 10, is1v1);

            // Notify clients about the leaderboard update
            try
            {
                if (_hubContext.Clients != null)
                {
                    UpdateLeaderboard(is1v1 ? "1v1" : "2v2").Wait();
                }
            }
            catch (AggregateException ex)
            {
                // Handle the exception
                throw ex.Flatten().InnerException;
            }
        }

        private void UpdateTeamEloForPlayers(TeamModel team, int opponentEloAverage, bool teamWon, bool is1v1)
        {
            // Calculate and update ELO for each player in the team
            foreach (UserModel user in new[] { team.User1, team.User2 }.Where(u => u != null))
            {
                // Determine which ELO rating (1v1 or 2v2) to update based on match type
                int currentElo = is1v1 ? user.Elo1v1 : user.Elo2v2;

                // Calculate the new ELO for the player based on the outcome of the match
                int newElo = CalculateNewElo(currentElo, opponentEloAverage, teamWon);

                // Update the player's ELO in the database
                UpdateUserElo(user.Id, newElo, is1v1);
            }
        }

        private int CalculateNewElo(int userElo, int opponentElo, bool won)
        {
            const int kFactor = 32; // The K-factor adjusts the sensitivity of ELO updates per match

            // Calculate expected score based on the player's ELO and the opponent's average ELO
            double expectedScore = 1.0 / (1.0 + Math.Pow(10, (opponentElo - userElo) / 400.0));

            // Actual score is 1 if the player won, or 0 if they lost
            double actualScore = won ? 1.0 : 0.0;

            // Calculate and return the updated ELO value
            return (int)(userElo + kFactor * (actualScore - expectedScore));
        }

        private bool UpdateUserElo(int userId, int elo, bool is1v1)
        {
            // Update the player's ELO in the database, specifying if it is a 1v1 or 2v2 rating
            return _userDatabaseAccessor.UpdateUserElo(userId, elo, is1v1);
        }

        public List<MatchHistoryModel> GetMatchHistoryByUserId(int userId)
        {
            return _userDatabaseAccessor.GetMatchHistoryByUserId(userId);
        }
    }
}