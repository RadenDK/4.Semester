﻿using System.Text.RegularExpressions;
using FoosballProLeague.Api.Models.FoosballModels;
using FoosballProLeague.Api.Models.RequestModels;

namespace FoosballProLeague.Api.BusinessLogic
{
    public interface IMatchLogic
    {
        public bool LoginOnTable(TableLoginRequest tableLoginRequest);

        public bool StartMatch(int tableId);

        public bool RegisterGoal(RegisterGoalRequest registerGoalRequest);

        public void InterruptMatch(int tableId);

        public TeamModel GetOrRegisterTeam(List<int?> userIds, int? existingTeamId = null);
        
        public List<MatchModel> GetAllMatches();
        public MatchModel GetActiveMatch();

    }

}
