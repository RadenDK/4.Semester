using FoosballProLeague.Api.Models;
using Dapper;
using System.Data;
using FoosballProLeague.Api.Models.FoosballModels;
using FoosballProLeague.Api.DatabaseAccess.Interfaces;
using FoosballProLeague.Api.Models.DbModels;

namespace FoosballProLeague.Api.DatabaseAccess
{
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
        public UserModel GetUserByEmail(string email)
        {
            UserModel user = null;

            string query = "SELECT * FROM Users WHERE email = @Email";

            using (IDbConnection connection = GetConnection())
            {
                connection.Open();
                user = connection.QuerySingleOrDefault<UserModel>(query, new { Email = email });
            }
            return user;
        }

        // This method is used to get a user by its id. It will return a UserModel object.
        public UserModel GetUserById(int userId)
        {
            string userQuery = "SELECT * FROM users WHERE id = @UserId";

            using (IDbConnection connection = GetConnection())
            {
                connection.Open();
                UserDbModel userDb = connection.QuerySingleOrDefault<UserDbModel>(userQuery, new { UserId = userId });

                if (userDb == null)
                {
                    return null;
                }

                // Map the userDb to a UserModel
                UserModel userModel = new UserModel
                {
                    Id = userDb.Id,
                    FirstName = userDb.FirstName,
                    LastName = userDb.LastName,
                    Email = userDb.Email,
                    Password = userDb.Password,
                    Elo1v1 = userDb.Elo1v1,
                    Elo2v2 = userDb.Elo2v2
                };

                return userModel;
            }
        }

        public List<UserModel> GetAllUsers()
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
    fm.id,
    fm.team_red_score AS RedTeamScore,
    fm.team_blue_score AS BlueTeamScore,
    TO_CHAR(fm.end_time, 'YYYY-MM-DD HH24:MI:SS') AS EndTime,

    -- Red Team Details
    red_team.id AS Id,
    red_team.user1_id AS RedUser1Id,
    red_team.user2_id AS RedUser2Id,
    u1.id AS RedUser1Id,
    u1.first_name,
    u1.last_name,
    u1.elo_1v1,
    u1.elo_2v2,
    u2.id AS RedUser2Id,
    u2.first_name,
    u2.last_name,
    u2.elo_1v1,
    u2.elo_2v2,

    -- Blue Team Details
    blue_team.id AS Id,
    blue_team.user1_id AS BlueUser1Id,
    blue_team.user2_id AS BlueUser2Id,
    u3.id AS BlueUser1Id,
    u3.first_name,
    u3.last_name,
    u3.elo_1v1,
    u3.elo_2v2,
    u4.id AS BlueUser2Id,
    u4.first_name,
    u4.last_name,
    u4.elo_1v1,
    u4.elo_2v2
FROM
    foosball_matches fm
        JOIN teams AS red_team ON fm.red_team_id = red_team.id
        LEFT JOIN users u1 ON red_team.user1_id = u1.id
        LEFT JOIN users u2 ON red_team.user2_id = u2.id
        JOIN teams AS blue_team ON fm.blue_team_id = blue_team.id
        LEFT JOIN users u3 ON blue_team.user1_id = u3.id
        LEFT JOIN users u4 ON blue_team.user2_id = u4.id
WHERE
    (red_team.user1_id = @userId OR red_team.user2_id = @userId
    OR blue_team.user1_id = @userId OR blue_team.user2_id = @userId)
    AND fm.end_time IS NOT NULL";

            using (IDbConnection connection = GetConnection())
            {
                IEnumerable<MatchHistoryModel> matches = connection.Query<MatchHistoryModel, TeamModel, UserModel, UserModel, TeamModel, UserModel, UserModel, MatchHistoryModel>(
                    query,
                    (match, redTeam, redUser1, redUser2, blueTeam, blueUser1, blueUser2) =>
                    {
                        match.RedTeam = redTeam;
                        match.BlueTeam = blueTeam;

                        match.RedTeam.User1 = redUser1;
                        match.RedTeam.User2 = redUser2;
                        match.BlueTeam.User1 = blueUser1;
                        match.BlueTeam.User2 = blueUser2;

                        return match;
                    },
                    new { UserId = userId },
                    splitOn: "Id,RedUser1Id,RedUser2Id,Id,BlueUser1Id,BlueUser2Id"
                );

                return matches.ToList();
            }
        }
    }
}
