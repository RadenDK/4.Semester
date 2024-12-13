namespace FoosballProLeague.Api.Models
{
    public class TournamentModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? WinnerTeamId { get; set; }
        public int MaxParticipants { get; set; }
    }   
}