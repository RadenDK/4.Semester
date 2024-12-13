using FoosballProLeague.Api.Models;

namespace FoosballProLeague.Api.BusinessLogic.Interfaces
{
    public interface ITournamentLogic
    {
        public Task<bool> CreateTournamentAsync(TournamentModel tournament);
        public Task<bool> JoinTournamentAsync(int tournamentId, int teamId);
    }
}