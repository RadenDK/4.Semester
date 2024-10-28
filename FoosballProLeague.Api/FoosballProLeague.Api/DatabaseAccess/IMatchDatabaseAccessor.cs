using FoosballProLeague.Api.Models.FoosballModels;
using FoosballProLeague.Api.Models;

namespace FoosballProLeague.Api.DatabaseAccess
{
    public interface IMatchDatabaseAccessor
    {
        public int? GetActiveMatchByTableId(int tableId);

        public int GetTeamIdByMatchId(int matchId, string teamSide);

        public bool LogGoal(MatchLogModel matchLog);

        public bool UpdateMatchScore(int matchId, int redScore, int blueScore);

        public bool SetTableActiveMatch(int tableId, int? matchId);

        public MatchModel GetMatchById(int matchId);

        public int CreateMatch(int tableId, int redTeamId, int blueTeamId);

        public int? GetTeamIdByPlayers(List<int> playerIds);

        public int RegisterTeam(List<int> playerIds);

        public bool EndMatch(int matchId);

        public List<UserModel> GetUsersByTeamId(int teamId);
    }
}
