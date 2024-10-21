namespace FoosballProLeague.Api.Models.FoosballModels
{
    public class MatchLogModel
    {
        public int MatchId { get; set; }
        public int TeamId { get; set; }
        public string Side { get; set; }

        public DateTime Log_time { get; set; }

    }
}
