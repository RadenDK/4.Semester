
namespace FoosballProLeague.Api.Models.FoosballModels
{
    public class MatchModel
    {
        public int Id { get; set; }

        public int TableId { get; set; }

        public TeamModel RedTeam { get; set; }

        public TeamModel BlueTeam { get; set; }

        public int RedTeamScore { get; set; }

        public int BlueTeamScore { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }
        
        public bool ValidEloMatch { get; set; }
    }
}
