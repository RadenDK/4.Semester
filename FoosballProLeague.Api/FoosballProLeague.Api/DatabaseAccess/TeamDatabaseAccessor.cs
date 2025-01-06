using FoosballProLeague.Api.Models.DbModels;
using FoosballProLeague.Api.Models.FoosballModels;
using FoosballProLeague.Api.Models;
using Npgsql;
using Dapper;
using FoosballProLeague.Api.DatabaseAccess.Interfaces;

namespace FoosballProLeague.Api.DatabaseAccess
{
    public class TeamDatabaseAccessor : DatabaseAccessor, ITeamDatabaseAccessor
    {
        private IUserDatabaseAccessor _userDatabaseAccessor;
        public TeamDatabaseAccessor(IConfiguration configuration, IUserDatabaseAccessor userDatabaseAccessor) : base(configuration)
        {
            _userDatabaseAccessor = userDatabaseAccessor;
        }

        // This helper method is used to get a team by its id. It will return a TeamModel object with UserModel objects nested inside.
        // It reuses the connection object to avoid opening and closing the connection multiple times.
        public TeamModel GetTeamById(int teamId, NpgsqlConnection? connection = null)
        {
            bool shouldCloseConnection = false;
            if (connection == null)
            {
                connection = GetConnection();
                connection.Open();
                shouldCloseConnection = true;
            }

            string teamQuery = "SELECT * FROM teams WHERE id = @TeamId";

            // 
            TeamDbModel teamDb = connection.QuerySingleOrDefault<TeamDbModel>(teamQuery, new { TeamId = teamId });
            
            
            if (teamDb == null)
            {
                if (shouldCloseConnection)
                {
                    connection.Close();
                }

                return null;
            }

            // Get user 1 which should be garentied to exist
            UserModel user1 = _userDatabaseAccessor.GetUserById(teamDb.User1Id);

            // Get user 2 if it exists
            UserModel user2 = teamDb.User2Id.HasValue ? _userDatabaseAccessor.GetUserById(teamDb.User2Id.Value) : null;

            // Map the teamDb to a TeamModel
            TeamModel team = new TeamModel
            {
                Id = teamDb.Id,
                User1 = user1,
                User2 = user2
            };

            if (shouldCloseConnection)
            {
                connection.Close();
            }

            return team;
        }

        // This method is used to get the team id for a list of player ids. It will return a TeamModel object with UserModel objects nested inside.
        public TeamModel GetTeamIdByUsers(IEnumerable<int> playerIds)
        {
            TeamModel team = null;

            string query = "";

            // If the playerIds only contain one player, then the team is a single player team and we query for that
            // Else we query for a team with two players
            if (playerIds.Count() == 1)
            {
                query = @"
                    SELECT id
                    FROM teams
                    WHERE (user1_id = @User1Id AND user2_id IS NULL);";
            }
            else
            {
                query = @"
                    SELECT id
                    FROM teams
                    WHERE (user1_id = @User1Id AND user2_id = @User2Id)
                       OR (user1_id = @User2Id AND user2_id = @User1Id);";
            }

            using (NpgsqlConnection connection = GetConnection())
            {
                connection.Open();

                int? teamId = connection.QuerySingleOrDefault<int?>(query, new { User1Id = playerIds.First(), User2Id = playerIds.Last() });

                // If the teamId is not null, get the team
                if (teamId.HasValue)
                {
                    team = GetTeamById(teamId.Value, connection);
                }

                // will be null if the team does not exist
                return team;
            }
        }

        public TeamModel CreateTeam(IEnumerable<int> userIds)
        {
            string query = "INSERT INTO teams (user1_id, user2_id) VALUES (@User1Id, @User2Id) RETURNING id";

            using (NpgsqlConnection connection = GetConnection())
            {
                connection.Open();

                List<int> userIdList = userIds.ToList();
                int user1Id = userIdList.First();
                int? user2Id = null;

                if (userIdList.Count > 1)
                {
                    user2Id = userIdList[1]; // Set the second user ID if it exists
                }

                int teamId = connection.QuerySingle<int>(query, new { User1Id = user1Id, User2Id = user2Id });

                TeamModel newTeam = GetTeamById(teamId, connection);

                return newTeam;
            }
        }

        public bool RemovePendingUser(int userId)
        {
            string query = "UPDATE table_login_requests SET status = 'removed' WHERE user_id = @UserId AND status = 'pending'";

            using (NpgsqlConnection connection = GetConnection())
            {
                connection.Open();

                int rowsAffected = connection.Execute(query, new { UserId = userId });

                return rowsAffected > 0;
            }
        }
    }
}
