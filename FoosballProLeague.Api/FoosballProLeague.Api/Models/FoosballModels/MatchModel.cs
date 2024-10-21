namespace FoosballProLeague.Api.Models.FoosballModels
{
    public class MatchModel
    {
        public int MatchId { get; set; }
        public int TableId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }   
        public int RedTeamId { get; set; }
        public int BlueTeamId { get; set; }
        public int RedScore { get; set; }
        public int BlueScore { get; set; }
    }
}
