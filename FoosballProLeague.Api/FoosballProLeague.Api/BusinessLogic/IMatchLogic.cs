using FoosballProLeague.Api.Models.RequestModels;

namespace FoosballProLeague.Api.BusinessLogic
{
    public interface IMatchLogic
    {
        public bool LoginOnTable(TableLoginRequest tableLoginRequest);

        public bool RegisterGoal(RegisterGoalRequest registerGoalRequest);

    }

}
