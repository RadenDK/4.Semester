using FoosballProLeague.Api.Models;
using Npgsql;
using Dapper;
using System.Data;
using FoosballProLeague.Api.Models.FoosballModels;
using Microsoft.AspNetCore.SignalR;

namespace FoosballProLeague.Api.DatabaseAccess;

public class UserDatabaseAccessor : DatabaseAccessor, IUserDatabaseAccessor
{
    
    public UserDatabaseAccessor(IConfiguration configuration) : base(configuration)
    {
    }
    
    // method to create user for registration
    public bool CreateUser(UserRegistrationModel newUserWithHashedPassword)
    {
        bool userInserted = false;
        
        string query = @"INSERT INTO users (first_name, last_name, email, password, department_Id, company_Id, elo_1v1, elo_2v2)
                        VALUES (@FirstName, @LastName, @Email, @Password, @DepartmentId, @CompanyId, @Elo1v1, @Elo2v2)";

        using (IDbConnection connection = GetConnection())
        {
            connection.Open();
            int rowsAffected = connection.Execute(query, newUserWithHashedPassword);
            userInserted = rowsAffected == 1;
        }
        return userInserted;
    }

    // method to login user / method to get user by email
    public UserModel GetUser(string email)
    {
        UserModel user = null;

        string query = "SELECT email, password FROM Users WHERE email = @Email";

        using (IDbConnection connection = GetConnection())
        {
            connection.Open();
            user = connection.QuerySingleOrDefault<UserModel>(query, new { Email = email });
        }
        return user;
    }

    public UserModel GetUserById(int userId)
    {
        UserModel user = null;

        string query = "SELECT id AS Id, first_name AS FirstName, last_name AS LastName, email AS Email, password AS Password, elo_1v1 AS Elo1v1, elo_2v2 AS Elo2v2 FROM Users WHERE id = @userId";
        
        using (IDbConnection connection = GetConnection())
        {
            connection.Open();
            user = connection.QuerySingleOrDefault<UserModel>(query, new { userId = userId });
        }
        return user;
    }

    public List<UserModel> GetUsers()
    {
        List<UserModel> users = new List<UserModel>();
        string query = "SELECT id, first_name, last_name, email, elo_1v1, elo_2v2 FROM users";

        using (IDbConnection connection = GetConnection())
        {
            connection.Open();
            users = connection.Query<UserModel>(query).ToList();
        }
        return users;
    }

    public bool UpdateUserElo(int userId, int elo, bool is1v1)
    {
        bool rowsAffected = false;
        string query;
        if (is1v1)
        {
            query = "UPDATE users SET elo_1v1 = @elo WHERE id = @UserId";
        }
        else
        {
            query = "UPDATE users SET elo_2v2 = @elo WHERE id = @UserId";
        }

        using (IDbConnection connection = GetConnection())
        {
            connection.Open();
            int affectedRows = connection.Execute(query, new { UserId = userId, elo });
            rowsAffected = affectedRows > 0;
        }

        return rowsAffected;
    }

    public List<MatchHistoryModel> GetMatchHistoryByUserId(int userId)
    {
        string query = @"
SELECT
    fm.id AS MatchId,
    fm.team_red_score AS TeamRedScore,
    fm.team_blue_score AS TeamBlueScore,
    TO_CHAR(fm.end_time, 'YYYY-MM-DD HH24:MI:SS') AS EndTime,

    -- Red Team Details
    red_team.id AS RedTeamId,
    red_team.player1_id AS RedPlayer1Id,
    red_team.player2_id AS RedPlayer2Id,
    u1.first_name AS RedPlayer1FirstName,
    u1.last_name AS RedPlayer1LastName,
    u1.elo_1v1 AS RedPlayer1Elo1v1,
    u1.elo_2v2 AS RedPlayer1Elo2v2,
    u2.first_name AS RedPlayer2FirstName,
    u2.last_name AS RedPlayer2LastName,
    u2.elo_1v1 AS RedPlayer2Elo1v1,
    u2.elo_2v2 AS RedPlayer2Elo2v2,

    -- Blue Team Details
    blue_team.id AS BlueTeamId,
    blue_team.player1_id AS BluePlayer1Id,
    blue_team.player2_id AS BluePlayer2Id,
    u3.first_name AS BluePlayer1FirstName,
    u3.last_name AS BluePlayer1LastName,
    u3.elo_1v1 AS BluePlayer1Elo1v1,
    u3.elo_2v2 AS BluePlayer1Elo2v2,
    u4.first_name AS BluePlayer2FirstName,
    u4.last_name AS BluePlayer2LastName,
    u4.elo_1v1 AS BluePlayer2Elo1v1,
    u4.elo_2v2 AS BluePlayer2Elo2v2
FROM
    foosball_matches fm
        JOIN teams AS red_team ON fm.red_team_id = red_team.id
        LEFT JOIN users u1 ON red_team.player1_id = u1.id
        LEFT JOIN users u2 ON red_team.player2_id = u2.id
        JOIN teams AS blue_team ON fm.blue_team_id = blue_team.id
        LEFT JOIN users u3 ON blue_team.player1_id = u3.id
        LEFT JOIN users u4 ON blue_team.player2_id = u4.id
WHERE
    red_team.player1_id = @userId OR red_team.player2_id = @userId
    OR blue_team.player1_id = @userId OR blue_team.player2_id = @userId";

        using (IDbConnection connection = GetConnection())
        {
            var matches = connection.Query<MatchHistoryModel, TeamModel, TeamModel, MatchHistoryModel>(
                query,
                (match, redTeam, blueTeam) =>
                {
                    match.RedTeam = redTeam;
                    match.BlueTeam = blueTeam;
                    return match;
                },
                new { userId },
                splitOn: "RedTeamId,BlueTeamId"
            );

            return matches.ToList();
        }
    }
}