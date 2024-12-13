using FoosballProLeague.Api.BusinessLogic.Interfaces;
using FoosballProLeague.Api.DatabaseAccess.Interfaces;
using FoosballProLeague.Api.Models;

namespace FoosballProLeague.Api.BusinessLogic
{
    public class TournamentLogic : ITournamentLogic
    {
        private readonly ITournamentDatabaseAccessor _tournamentDatabaseAccessor;

        public TournamentLogic(ITournamentDatabaseAccessor tournamentDatabaseAccessor)
        {
            _tournamentDatabaseAccessor = tournamentDatabaseAccessor;
        }
        
        public async Task<bool> CreateTournamentAsync(TournamentModel tournament)
        {
            return await _tournamentDatabaseAccessor.CreateTournamentAsync(tournament);
        }

        public async Task<bool> JoinTournamentAsync(int tournamentId, int teamId)
        {
            return await _tournamentDatabaseAccessor.JoinTournamentAsync(tournamentId, teamId);
        }
    }
}