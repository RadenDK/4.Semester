using FoosballProLeague.Api.Models.FoosballModels;
using FoosballProLeague.Api.Models;

namespace FoosballProLeague.Api.DatabaseAccess
{
    public interface IMatchDatabaseAccessor
    {
        // Match-related methods
        public MatchModel GetMatchById(int matchId);
        public int CreateMatch(int tableId, int redTeamId, int blueTeamId);
        public bool UpdateMatchScore(int matchId, int redScore, int blueScore);
        public bool EndMatch(int matchId);

        // Team-related methods
        public int GetTeamIdByMatchId(int matchId, string teamSide);
        public int? GetTeamIdByPlayers(List<int?> playerIds);
        public int RegisterTeam(List<int?> playerIds);
        public TeamModel GetTeamById(int teamId);
        public bool UpdateTeamId(int matchId, string teamSide, int teamId);

        // Table-related methods
        public int? GetActiveMatchIdByTableId(int tableId);
        public bool SetTableActiveMatch(int tableId, int? matchId);

        // Goal-related methods
        public bool LogGoal(MatchLogModel matchLog);

        public List<UserModel> GetUsersByTeamId(int teamId);
    }
}
