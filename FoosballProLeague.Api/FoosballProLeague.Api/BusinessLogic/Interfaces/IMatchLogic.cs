using FoosballProLeague.Api.Models;
using FoosballProLeague.Api.Models.FoosballModels;
using FoosballProLeague.Api.Models.RequestModels;

namespace FoosballProLeague.Api.BusinessLogic.Interfaces
{
    public interface IMatchLogic
    {
        public bool LoginOnTable(TableLoginRequest tableLoginRequest);

        public bool StartMatch(int tableId);

        public bool RegisterGoal(RegisterGoalRequest registerGoalRequest);

        public void InterruptMatch(int tableId);

        public List<MatchModel> GetAllMatches();

        public MatchModel GetActiveMatch();
        public List<TableLoginRequest> GetPendingTeamUsers(int tableId);
        public bool RemovePendingUser(string email);

        public void ClearPendingTeamsCache();
    }
}
