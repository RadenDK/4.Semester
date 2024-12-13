using FoosballProLeague.Api.Models;

namespace FoosballProLeague.Api.DatabaseAccess.Interfaces
{
    public interface ITournamentDatabaseAccessor
    {
        public Task<bool> CreateTournamentAsync(TournamentModel tournament);
        public Task<bool> JoinTournamentAsync(int tournamentId, int teamId);
    }
}