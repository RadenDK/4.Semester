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
        public TeamModel GetTeamById(NpgsqlConnection connection, int teamId)
        {
            string teamQuery = "SELECT * FROM teams WHERE id = @TeamId";

            // 
            TeamDbModel teamDb = connection.QuerySingleOrDefault<TeamDbModel>(teamQuery, new { TeamId = teamId });
            if (teamDb == null)
            {
                return null;
            }

            // Get user 1 which should be garentied to exist
            UserModel user1 = _userDatabaseAccessor.GetUserById(teamDb.Player1Id);

            // Get user 2 if it exists
            UserModel user2 = teamDb.Player2Id.HasValue ? _userDatabaseAccessor.GetUserById(teamDb.Player2Id.Value) : null;

            // Map the teamDb to a TeamModel
            TeamModel team = new TeamModel
            {
                Id = teamDb.Id,
                User1 = user1,
                User2 = user2
            };

            return team;
        }

        // This method is used to get the team id for a list of player ids. It will return a TeamModel object with UserModel objects nested inside.
        public TeamModel GetTeamIdByUsers(List<int?> playerIds)
        {
            TeamModel team = null;

            string query = "";

            // If the playerIds only contain one player, then the team is a single player team and we query for that
            // Else we query for a team with two players
            if (playerIds.Last() == null)
            {
                query = @"
                    SELECT id
                    FROM teams
                    WHERE (player1_id = @Player1Id AND player2_id IS NULL);";
            }
            else
            {
                query = @"
                    SELECT id
                    FROM teams
                    WHERE (player1_id = @Player1Id AND player2_id = @Player2Id)
                       OR (player1_id = @Player2Id AND player2_id = @Player1Id);";
            }

            using (NpgsqlConnection connection = GetConnection())
            {
                connection.Open();
                int? teamId = connection.QuerySingleOrDefault<int?>(query, new { Player1Id = playerIds[0], Player2Id = playerIds[1] });

                // If the teamId is not null, get the team
                if (teamId.HasValue)
                {
                    team = GetTeamById(connection, teamId.Value);
                }

                // will be null if the team does not exist
                return team;
            }
        }

        public TeamModel CreateTeam(List<int?> playerIds)
        {
            string query = "INSERT INTO teams (player1_id, player2_id) VALUES (@Player1Id, @Player2Id) RETURNING id";

            using (NpgsqlConnection connection = GetConnection())
            {
                connection.Open();
                int teamId = connection.QuerySingle<int>(query, new { Player1Id = playerIds[0], Player2Id = playerIds[1] });

                TeamModel newTeam = GetTeamById(connection, teamId);

                return newTeam;
            }
        }
    }
}
