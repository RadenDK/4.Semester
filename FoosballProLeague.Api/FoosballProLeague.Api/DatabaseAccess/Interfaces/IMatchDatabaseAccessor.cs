using FoosballProLeague.Api.Models.FoosballModels;

namespace FoosballProLeague.Api.DatabaseAccess.Interfaces
{
    public interface IMatchDatabaseAccessor
    {
        // Create methods
        public int CreateMatch(int tableId, int redTeamId, int blueTeamId, bool? validEloMatch = null);
        public bool CreateMatchLog(MatchLogModel matchLog);

        // Read methods
        public MatchModel GetMatchById(int matchId);
        public MatchModel GetActiveMatchByTableId(int tableId);

        // Update methods
        public bool UpdateMatchScore(MatchModel match);
        public bool UpdateMatchTeamIds(MatchModel match);
        public bool UpdateValidEloMatch(int matchId, bool validEloMatch);
        public bool UpdateTableActiveMatch(int tableId, int? matchId);
        public bool EndMatch(int matchId);
    }
}
