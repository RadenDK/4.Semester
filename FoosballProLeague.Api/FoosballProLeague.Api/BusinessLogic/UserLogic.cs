using FoosballProLeague.Api.Models;
using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.Models.FoosballModels;
using bc = BCrypt.Net.BCrypt;
using Microsoft.AspNetCore.SignalR;
using FoosballProLeague.Api.Hubs; 

namespace FoosballProLeague.Api.BusinessLogic;

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
                Email = userRegistrationModel.Email,
                Password = bc.HashPassword(userRegistrationModel.Password),
                DepartmentId = userRegistrationModel.DepartmentId,
                CompanyId = userRegistrationModel.CompanyId,
                Elo1v1 = 500,
                Elo2v2 = 500
            };
            bool userCreated = _userDatabaseAccessor.CreateUser(newUserWithHashedPassword);
            if (userCreated)
            {
                // Notify clients about the leaderboard update
                UpdateLeaderboard().Wait();
            }
            return userCreated;
        }
        return false;
    }

    private async Task UpdateLeaderboard()
    {
        var users = _userDatabaseAccessor.GetUsers();
        var leaderboard = users.OrderByDescending(u => u.Elo1v1).ToList();
        await _hubContext.Clients.All.SendAsync("ReceiveLeaderboardUpdate", leaderboard);
    }
    
    // checks if the account has values
    private bool AccountHasValues(UserRegistrationModel newUser)
    {
        if (newUser == null)
        {
            return false;
        }
        
        if (string.IsNullOrEmpty(newUser.FirstName))
        {
            return false;
        }

        if (string.IsNullOrEmpty(newUser.LastName))
        {
            return false;
        }

        if (string.IsNullOrEmpty(newUser.Email) && _userDatabaseAccessor.GetUser(newUser.Email) != null)
        {
            return false;
        }

        if (string.IsNullOrEmpty(newUser.Password))
        {
            return false;
        }

        return true;
    }
    
    //method to login user
    public bool LoginUser(string email, string password)
    {
        UserModel user = _userDatabaseAccessor.GetUser(email);
        if (user == null || !bc.Verify(password, user.Password))
        {
            return false;
        }
        return bc.Verify(password, user.Password);
    }
    
    // get all user in a list
    public List<UserModel> GetUsers()
    {
        return _userDatabaseAccessor.GetUsers();
    }

    public UserModel GetUser(string email)
    {
        return _userDatabaseAccessor.GetUser(email);
    }

    public UserModel GetUserById(int userId)
    {
        return _userDatabaseAccessor.GetUserById(userId);
    }
    
    public void UpdateTeamElo(TeamModel redTeam, TeamModel blueTeam, bool redTeamWon, bool is1v1)
    {
        // Check if the match is a valid 1v1 or 2v2
        if ((is1v1 && (redTeam.User2 != null || blueTeam.User2 != null)) || (!is1v1 && (redTeam.User2 == null || blueTeam.User2 == null)))
        {
            // Invalid match configuration
            return;
        }
        
        // Calculate average ELO for each team
        // this
        // int redTeamElo = is1v1 ? redTeam.User1.Elo1v1 : (redTeam.User1.Elo2v2 + redTeam.User2.Elo2v2) / 2;
        // or this?
        int redTeamElo;
        if (is1v1)
        {
            redTeamElo = redTeam.User1.Elo1v1;
        }
        else
        {
            redTeamElo = (redTeam.User1.Elo2v2 + redTeam.User2.Elo2v2) / 2;
        }
        
        // this
        // int blueTeamElo = is1v1 ? blueTeam.User1.Elo1v1 : (blueTeam.User1.Elo2v2 + blueTeam.User2.Elo2v2) / 2;
        // or this?
        int blueTeamElo;
        if (is1v1)
        {
            blueTeamElo = blueTeam.User1.Elo1v1;
        }
        else
        {
            blueTeamElo = (blueTeam.User1.Elo2v2 + blueTeam.User2.Elo2v2) / 2;
        }
        
        // Update ELO for each player in the red team
        foreach (UserModel user in new []{ redTeam.User1, redTeam.User2 }.Where(u => u != null)) // u => u != null avoids NullReferenceExceptions by only including elements that arent null. Can be removed if seen as unessecary
        {
            int newElo = CalculateNewElo(is1v1 ? user.Elo1v1 : user.Elo2v2, blueTeamElo, redTeamWon);
            UpdateUserElo(user.Id, newElo, is1v1);
        }
        
        // Update ELO for each player in the blue team
        foreach (UserModel user in new []{ blueTeam.User1, blueTeam.User2 }.Where(u => u != null))
        {
            int newElo = CalculateNewElo(is1v1 ? user.Elo1v1 : user.Elo2v2, redTeamElo, !redTeamWon);
            UpdateUserElo(user.Id, newElo, is1v1);
        }
    }
    
    private int CalculateNewElo(int userElo, int opponentElo, bool won)
    {
        const int kFactor = 32; // K-factor determines the maximum possible adjustment per game
        
        // Calculate expected score
        double expectedScore = 1.0 / (1.0 + Math.Pow(10, (opponentElo - userElo) / 400.0));
        
        // Determine actual score
        double actualScore = won ? 1.0 : 0.0;
        
        // Calculate new ELO
        int newElo = (int)(userElo + kFactor * (actualScore - expectedScore));

        return newElo;
    }
    
    private bool UpdateUserElo(int userId, int elo, bool is1v1)
    {
        return _userDatabaseAccessor.UpdateUserElo(userId, elo, is1v1);
    }
}