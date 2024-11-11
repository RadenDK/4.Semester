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
    
    public List<TeamModel> GetTeamsByUserId(int userId)
    {
        string query = @"
        SELECT
            t.id,
            t.player1_id AS UserId, u1.id, u1.first_name, u1.last_name,
            u1.elo_1v1, u1.elo_2v2,
            t.player2_id AS UserId, u2.id, u2.first_name, u2.last_name,
            u2.elo_1v1, u2.elo_2v2
        FROM
            teams t
        JOIN users u1 ON t.player1_id = u1.id
        LEFT JOIN users u2 ON t.player2_id = u2.id
        WHERE
            t.player1_id = @userId OR t.player2_id = @userId";

        using (IDbConnection connection = GetConnection())
        {
            return connection.Query<TeamModel, UserModel, UserModel, TeamModel>(
                query,
                (team, user1, user2) =>
                {
                    team.User1 = user1;
                    team.User2 = user2 ?? new UserModel();
                    return team;
                },
                new { UserId = userId },
                splitOn: "UserId,UserId"
            ).ToList();
        }
    }
    
    public List<MatchHistoryModel> GetMatchIdByTeamId(int teamId)
    {
        string query = @"
        SELECT
            fm.id,
            fm.team_red_score AS TeamRedScore,
            fm.team_blue_score AS TeamBlueScore,
            TO_CHAR(fm.end_time, 'YYYY-MM-DD HH24:MI:SS') AS EndTime,
            red_team.id AS RedTeamId,
            red_team.player1_id,
            red_team.player2_id,
            blue_team.id AS BlueTeamId,
            blue_team.player1_id,
            blue_team.player2_id
        FROM
            foosball_matches fm
            JOIN teams AS red_team ON fm.red_team_id = red_team.id
            JOIN teams AS blue_team ON fm.blue_team_id = blue_team.id
        WHERE
            fm.red_team_id = @teamId OR fm.blue_team_id = @teamId";

        using (IDbConnection connection = GetConnection())
        {
            IEnumerable<MatchHistoryModel> matches = connection.Query<MatchHistoryModel, TeamModel, TeamModel, MatchHistoryModel>(
                query,
                (match, redTeam, blueTeam) =>
                {
                    match.RedTeam = redTeam;
                    match.BlueTeam = blueTeam;
                    return match;
                },
                new { TeamId = teamId },
                splitOn: "RedTeamId,BlueTeamId"
            );
            
            return matches.ToList();
        }
    }
}