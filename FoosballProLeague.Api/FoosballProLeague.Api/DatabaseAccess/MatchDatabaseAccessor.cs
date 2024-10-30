using Dapper;
using FoosballProLeague.Api.Models.FoosballModels;
using FoosballProLeague.Api.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Text.RegularExpressions;

namespace FoosballProLeague.Api.DatabaseAccess
{
    public class MatchDatabaseAccessor : IMatchDatabaseAccessor
    {
        private readonly string _connectionString;

        public MatchDatabaseAccessor(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DatabaseConnection");
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        }

        // GET METHODS

        public int? GetActiveMatchIdByTableId(int tableId)
        {
            string query = "SELECT active_match_id FROM foosball_tables WHERE id = @TableId";

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                int? activeMatchId = connection.QuerySingleOrDefault<int?>(query, new { TableId = tableId });
                return activeMatchId;
            }
        }

        public int GetTeamIdByMatchId(int matchId, string teamSide)
        {
            // Validate the teamSide input
            if (teamSide != "red" && teamSide != "blue")
            {
                throw new ArgumentException("Invalid team side. Allowed values are 'red' or 'blue'.");
            }

            // Construct the query using the validated teamSide
            string query = $"SELECT {teamSide}_team_id FROM foosball_matches WHERE id = @MatchId";

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                int teamId = connection.QuerySingleOrDefault<int>(query, new { MatchId = matchId });
                return teamId;
            }
        }


        public MatchModel GetMatchById(int matchId)
        {
            string query = "SELECT * FROM foosball_matches WHERE id = @MatchId";

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                // Retrieve the raw result set as a dynamic object
                var resultSet = connection.Query(query, new { MatchId = matchId });


                // Map the result to MatchModel
                MatchModel match = connection.QuerySingleOrDefault<MatchModel>(query, new { MatchId = matchId });
                return match;
            }
        }


        public int? GetTeamIdByPlayers(List<int?> playerIds)
        {
            string query = @"
                    SELECT id 
                    FROM teams 
                    WHERE player1_id = @Player1Id 
                        AND (player2_id = @Player2Id OR @Player2Id IS NULL)";

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                int? teamId = connection.QuerySingleOrDefault<int?>(query, new { Player1Id = playerIds[0], Player2Id = playerIds[1] });
                return teamId;
            }
        }


        public TeamModel GetTeamById(int teamId)
        {
            string query = @"
        SELECT 
            teams.id,
            user1.id, user1.first_name, user1.last_name, user1.elo_1v1, user1.elo_2v2,
            user2.id, user2.first_name, user2.last_name, user2.elo_1v1, user2.elo_2v2
        FROM
            teams
        LEFT JOIN
            users user1 ON teams.player1_id = user1.id
        LEFT JOIN
            users user2 ON teams.player2_id = user2.id
        WHERE
            teams.id = @TeamId";

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                TeamModel team = connection.Query<TeamModel, UserModel, UserModel, TeamModel>(
                    query,
                    (teamResult, user1, user2) =>
                    {
                        teamResult.User1 = user1;
                        teamResult.User2 = user2;
                        return teamResult;
                    },
                    new { TeamId = teamId },
                    splitOn: "id,id"  // Dapper needs this to split at user1.id and user2.id
                ).FirstOrDefault();

                return team;
            }
        }


        // CREATE METHODS

        public int CreateMatch(int tableId, int redTeamId, int blueTeamId)
        {
            string query = "INSERT INTO foosball_matches (table_id, red_team_id, blue_team_id) VALUES (@TableId, @RedTeamId, @BlueTeamId) RETURNING id";

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                int matchId = connection.QuerySingle<int>(query, new { TableId = tableId, RedTeamId = redTeamId, BlueTeamId = blueTeamId });
                return matchId;
            }
        }

        public int RegisterTeam(List<int?> playerIds)
        {
            string query = "INSERT INTO teams (player1_id, player2_id) VALUES (@Player1Id, @Player2Id) RETURNING id";

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                int teamId = connection.QuerySingle<int>(query, new { Player1Id = playerIds[0], Player2Id = playerIds[1] });
                return teamId;
            }
        }

        public bool LogGoal(MatchLogModel matchLog)
        {
            string query = "INSERT INTO match_logs (match_id, team_id, side, log_time) VALUES (@MatchId, @TeamId, @Side, @LogTime)";

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                int rowsAffected = connection.Execute(query, matchLog);
                return rowsAffected > 0;
            }
        }

        // UPDATE METHODS

        public bool UpdateMatchScore(int matchId, int teamRedScore, int teamBlueScore)
        {
            string query = "UPDATE foosball_matches SET team_red_score = @TeamRedScore, team_blue_score = @TeamBlueScore WHERE id = @MatchId";

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                int rowsAffected = connection.Execute(query, new { MatchId = matchId, TeamRedScore = teamRedScore, TeamBlueScore = teamBlueScore });
                return rowsAffected > 0;
            }
        }

        public bool SetTableActiveMatch(int tableId, int? matchId)
        {
            string query = "UPDATE foosball_tables SET active_match_id = @MatchId WHERE id = @TableId";

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                int rowsAffected = connection.Execute(query, new { TableId = tableId, MatchId = matchId });
                return rowsAffected > 0;
            }
        }

        public bool EndMatch(int matchId)
        {
            string query = "UPDATE foosball_matches SET end_time = @EndTime WHERE id = @MatchId";

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                int rowsAffected = connection.Execute(query, new { MatchId = matchId, EndTime = DateTime.Now });
                return rowsAffected > 0;
            }
        }

        public bool UpdateTeamId(int matchId, string teamSide, int teamId)
        {
            // Validate the teamSide input
            if (teamSide != "red" && teamSide != "blue")
            {
                throw new ArgumentException("Invalid team side. Allowed values are 'red' or 'blue'.");
            }

            string query = $"UPDATE foosball_matches SET {teamSide}_team_id = @TeamId WHERE id = @MatchId";

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                int rowsAffected = connection.Execute(query, new { MatchId = matchId, TeamId = teamId });
                return rowsAffected > 0;
            }

        }
    }
}
