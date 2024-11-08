using FoosballProLeague.Api.Models.FoosballModels;

namespace FoosballProLeague.Api.DatabaseAccess
{
    public interface IMatchDatabaseAccessor
    {
        // Match-related methods
        public MatchModel GetMatchById(int matchId);
        public int CreateMatch(int tableId, int redTeamId, int blueTeamId, bool? validEloMatch = null);
        public bool UpdateMatchScore(int matchId, int redScore, int blueScore);
        public bool EndMatch(int matchId);
        public bool UpdateValidEloMatch(int matchId, bool validEloMatch);

        // Team-related methods
        public int GetTeamIdByMatchId(int matchId, string teamSide);
        public int? GetTeamIdByUsers(List<int?> playerIds);
        public int RegisterTeam(List<int?> playerIds);
        public TeamModel GetTeamById(int teamId);
        public bool UpdateUserIdOnTeamByTeamId(int? teamId, int userId);

        // Table-related methods
        public int? GetActiveMatchIdByTableId(int tableId);
        public bool SetTableActiveMatch(int tableId, int? matchId);

        // Goal-related methods
        public bool LogGoal(MatchLogModel matchLog);
    }
}
