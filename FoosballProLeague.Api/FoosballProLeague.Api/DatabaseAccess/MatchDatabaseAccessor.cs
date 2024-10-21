using Dapper;
using FoosballProLeague.Api.Models.FoosballModels;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using System;
using System.Data.SqlClient;

namespace FoosballProLeague.Api.DatabaseAccess
{
    public class MatchDatabaseAccessor : IMatchDatabaseAccessor
    {
        private readonly string _connectionString;

        public MatchDatabaseAccessor(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DatabaseConnection");
        }

        public int? GetActiveMatchByTableId(int tableId)
        {
            string query = "SELECT active_match_id FROM foosball_tables WHERE id = @TableId";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Use Dapper to execute the query and return the result
                int? activeMatchId = connection.QuerySingleOrDefault<int?>(query, new { TableId = tableId });

                return activeMatchId;
            }
        }

        public int GetTeamIdByMatchId(int matchId, string teamSide)
        {
            string query = "SELECT " + teamSide + "_team_id FROM foosall_matches WHERE match_id = @MatchId";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Use Dapper to execute the query and return the result
                int teamId = connection.QuerySingleOrDefault<int>(query, new { MatchId = matchId });

                return teamId;
            }
        }

        public bool LogGoal(MatchLogModel matchLog)
        {
            string query = "INSERT INTO match_log (match_id, team_id, side, log_time) VALUES (@MatchId, @TeamId, @Side, @LogTime)";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Use Dapper to execute the query and return the result
                int rowsAffected = connection.Execute(query, matchLog);

                return rowsAffected > 0;
            }
        }

        public int CreateMatch(int tableId, int redTeamId, int blueTeamId)
        {
            string query = "INSERT INTO foosball_matches (table_id, red_team_id, blue_team_id) OUTPUT INSERTED.match_id VALUES (@TableId, @RedTeamId, @BlueTeamId)";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Use Dapper to execute the query and return the result
                int matchId = connection.QuerySingle<int>(query, new { TableId = tableId, RedTeamId = redTeamId, BlueTeamId = blueTeamId });

                return matchId;
            }
        }

        public int? GetTeamIdByPlayers(List<int> playerIds)
        {
            string query = "SELECT team_id FROM teams WHERE player1_id IN @Players AND player2_id IN @PlayerIds";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Use Dapper to execute the query and return the result
                int? teamId = connection.QuerySingleOrDefault<int?>(query, new { PlayerIds = playerIds });

                return teamId;
            }
        }

        public int RegisterTeam(List<int> playerIds)
        {
            string query = "INSERT INTO teams (player1_id, player2_id) OUTPUT INSERTED.team_id VALUES (@Player1Id, @Player2Id)";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Use Dapper to execute the query and return the result
                int teamId = connection.QuerySingle<int>(query, new { Player1Id = playerIds[0], Player2Id = playerIds[1] });

                return teamId;
            }
        }

        public bool UpdateMatchScore(int matchId, int teamRedScore, int teamBlueScore)
        {
            string query = "UPDATE foosball_matches SET team_red_score = @TeamRedScore, team_blue_score = @TeamBlueScore WHERE match_id = @MatchId";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Use Dapper to execute the query and return the result
                int rowsAffected = connection.Execute(query, new { MatchId = matchId, TeamRedScore = teamRedScore, TeamBlueScore = teamBlueScore });

                return rowsAffected > 0;
            }
        }

        public bool SetTableActiveMatch(int tableId, int? matchId)
        {
            string query = "UPDATE foosball_tables SET active_match_id = @MatchId WHERE id = @TableId";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Use Dapper to execute the query and return the result
                int rowsAffected = connection.Execute(query, new { TableId = tableId, MatchId = matchId });

                return rowsAffected > 0;
            }
        }

        public MatchModel GetMatchById(int matchId)
        {
            string query = "SELECT * FROM foosball_matches WHERE match_id = @MatchId";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Use Dapper to execute the query and return the result
                MatchModel match = connection.QuerySingleOrDefault<MatchModel>(query, new { MatchId = matchId });

                return match;
            }
        }

        public bool EndMatch(int matchId)
        {
            string query = "UPDATE foosball_matches SET end_time = @EndTime WHERE match_id = @MatchId";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Use Dapper to execute the query and return the result
                int rowsAffected = connection.Execute(query, new { MatchId = matchId, EndTime = DateTime.Now });

                return rowsAffected > 0;
            }
        }
    }
}
