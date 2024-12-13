using FoosballProLeague.Api.DatabaseAccess.Interfaces;
using FoosballProLeague.Api.Models;
using System.Data;
using Dapper;

namespace FoosballProLeague.Api.DatabaseAccess
{
    public class TournamentDatabaseAccessor : DatabaseAccessor, ITournamentDatabaseAccessor
    {
        public TournamentDatabaseAccessor(IConfiguration configuration) : base(configuration)
        {
            
        }

        public async Task<bool> CreateTournamentAsync(TournamentModel tournament)
        {
            string query =
                @"INSERT INTO tournaments (name, start_time, end_time, winner_team_id, max_participants) VALUES (@Name, @StartTime, NULL, NULL, @MaxParticipants)";

            using (IDbConnection connection = GetConnection())
            {
                connection.Open();
                int rowsAffected = await connection.ExecuteAsync(query, tournament);
                return rowsAffected == 1;
            }
        }

        public async Task<bool> JoinTournamentAsync(int tournamentId, int teamId)
        {
            string query =
                @"INSERT INTO brackets (tournament_id, stage, match_id, team_red_id, team_blue_id) VALUES (@TournamentId, 1, NULL, @TeamId, NULL)";

            using (IDbConnection connection = GetConnection())
            {
                connection.Open();
                int rowsAffected =
                    await connection.ExecuteAsync(query, new { TournamentId = tournamentId, TeamId = teamId });
                return rowsAffected == 1;
            }
        }
    }
}