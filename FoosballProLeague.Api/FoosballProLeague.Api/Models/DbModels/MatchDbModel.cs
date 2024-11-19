namespace FoosballProLeague.Api.Models.DbModels
{
    public class MatchDbModel
    {
        public int Id { get; set; }

        public int TableId { get; set; }

        public int RedTeamId { get; set; }

        public int BlueTeamId { get; set; }

        public int TeamRedScore { get; set; }

        public int TeamBlueScore { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public bool ValidEloMatch { get; set; }
    }
}
