using Dapper;
using FoosballProLeague.Api.Models.FoosballModels;
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
            string query = "SELECT id FROM teams WHERE player1_id = @Player1Id AND player2_id = @Player2Id";

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                int? teamId = connection.QuerySingleOrDefault<int?>(query, new { Player1Id = playerIds[0], Player2Id = playerIds[1] });
                return teamId;
            }
        }

        public TeamModel GetTeamById(int teamId)
        {
            string query = "SELECT * FROM teams WHERE id = @TeamId";

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                TeamModel team = connection.QuerySingleOrDefault<TeamModel>(query, new { TeamId = teamId });
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
