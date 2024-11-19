using Dapper;
using FoosballProLeague.Api.Models.FoosballModels;
using Npgsql;
using FoosballProLeague.Api.DatabaseAccess.Interfaces;
using FoosballProLeague.Api.Models.DbModels;

namespace FoosballProLeague.Api.DatabaseAccess
{
    public class MatchDatabaseAccessor : DatabaseAccessor, IMatchDatabaseAccessor
    {
        private readonly ITeamDatabaseAccessor _teamDatabaseAccessor;
        public MatchDatabaseAccessor(IConfiguration configuration, ITeamDatabaseAccessor teamDatabaseAccessor) : base(configuration)
        {
            _teamDatabaseAccessor = teamDatabaseAccessor;
        }

        // This method is used to get a match by its id. It will return a MatchModel object with TeamModel and UserModel objects nested inside.
        public MatchModel GetMatchById(int matchId)
        {
            string matchQuery = "SELECT * FROM foosball_matches WHERE id = @MatchId";

            using (NpgsqlConnection connection = GetConnection())
            {
                connection.Open();

                // Retrieve the match
                MatchDbModel matchDb = connection.QuerySingleOrDefault<MatchDbModel>(matchQuery, new { MatchId = matchId });
                if (matchDb == null)
                {
                    return null;
                }

                // Retrieve the red team
                TeamModel redTeam = _teamDatabaseAccessor.GetTeamById(connection, matchDb.RedTeamId);

                // Retrieve the blue team
                TeamModel blueTeam = _teamDatabaseAccessor.GetTeamById(connection, matchDb.BlueTeamId);

                // Map the matchDb to a MatchModel
                MatchModel match = new MatchModel
                {
                    Id = matchDb.Id,
                    TableId = matchDb.TableId,
                    RedTeam = redTeam,
                    BlueTeam = blueTeam,
                    TeamRedScore = matchDb.TeamRedScore,
                    TeamBlueScore = matchDb.TeamBlueScore,
                    StartTime = matchDb.StartTime,
                    EndTime = matchDb.EndTime,
                    ValidEloMatch = matchDb.ValidEloMatch
                };

                return match;
            }
        }

        public List<MatchModel> GetAllMatches()
        {
            string query = "SELECT * FROM foosball_matches";

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                List<MatchModel> matches = connection.Query<MatchModel>(query).ToList();
                return matches;
            }
        }

        // This method is used to get the active match for a table by its id. It will return a MatchModel object with TeamModel and UserModel objects nested inside.
        public MatchModel GetActiveMatchByTableId(int tableId)
        {
            string query = "SELECT active_match_id FROM foosball_tables WHERE id = @TableId";

            MatchModel activeMatch = null;

            using (NpgsqlConnection connection = GetConnection())
            {
                connection.Open();
                int? activeMatchId = connection.QuerySingleOrDefault<int?>(query, new { TableId = tableId });

                // If there is an active match, get the match
                if (activeMatchId.HasValue)
                {
                    activeMatch = GetMatchById(activeMatchId.Value);
                }

                // This will be null if there is no active match
                return activeMatch;
            }
        }

        // CREATE METHODS
        public int CreateMatch(int tableId, int redTeamId, int blueTeamId, bool? validEloMatch = null)
        {
            string query = "INSERT INTO foosball_matches (table_id, red_team_id, blue_team_id, valid_elo_match) VALUES (@TableId, @RedTeamId, @BlueTeamId, @ValidEloMatch) RETURNING id";

            using (NpgsqlConnection connection = GetConnection())
            {
                connection.Open();
                int matchId = connection.QuerySingle<int>(query, new { TableId = tableId, RedTeamId = redTeamId, BlueTeamId = blueTeamId, ValidEloMatch = validEloMatch });
                return matchId;
            }
        }
        public bool CreateMatchLog(MatchLogModel matchLog)
        {
            string query = "INSERT INTO match_logs (match_id, team_id, side, log_time) VALUES (@MatchId, @TeamId, @Side, @LogTime)";

            using (NpgsqlConnection connection = GetConnection())
            {
                connection.Open();
                int rowsAffected = connection.Execute(query, matchLog);
                return rowsAffected > 0;
            }
        }

        // UPDATE METHODS

        public bool UpdateMatchScore(MatchModel match)
        {
            string query = "UPDATE foosball_matches SET team_red_score = @TeamRedScore, team_blue_score = @TeamBlueScore WHERE id = @MatchId";

            using (NpgsqlConnection connection = GetConnection())
            {
                connection.Open();
                int rowsAffected = connection.Execute(query, new { MatchId = match.Id, TeamRedScore = match.TeamRedScore, TeamBlueScore = match.TeamBlueScore });
                return rowsAffected > 0;
            }
        }

        public bool UpdateTableActiveMatch(int tableId, int? matchId)
        {
            string query = "UPDATE foosball_tables SET active_match_id = @MatchId WHERE id = @TableId";

            using (NpgsqlConnection connection = GetConnection())
            {
                connection.Open();
                int rowsAffected = connection.Execute(query, new { TableId = tableId, MatchId = matchId });
                return rowsAffected > 0;
            }
        }

        public bool EndMatch(int matchId)
        {
            string query = "UPDATE foosball_matches SET end_time = @EndTime WHERE id = @MatchId";

            using (NpgsqlConnection connection = GetConnection())
            {
                connection.Open();
                int rowsAffected = connection.Execute(query, new { MatchId = matchId, EndTime = DateTime.Now });
                return rowsAffected > 0;
            }
        }

        public bool UpdateValidEloMatch(int matchId, bool validEloMatch)
        {
            string query = "UPDATE foosball_matches SET valid_elo_match = @ValidEloMatch WHERE id = @MatchId";

            using (NpgsqlConnection connection = GetConnection())
            {
                connection.Open();
                int rowsAffected = connection.Execute(query, new { ValidEloMatch = validEloMatch, MatchId = matchId });
                return rowsAffected > 0;
            }
        }

        public bool UpdateMatchTeamIds(MatchModel match)
        {
            string query = "UPDATE foosball_matches SET red_team_id = @RedTeamId, blue_team_id = @BlueTeamId WHERE id = @MatchId";

            using (NpgsqlConnection connection = GetConnection())
            {
                connection.Open();
                int rowsAffected = connection.Execute(query, new { RedTeamId = match.RedTeam.Id, BlueTeamId = match.BlueTeam.Id, MatchId = match.Id });
                return rowsAffected > 0;
            }
        }
    }
}
