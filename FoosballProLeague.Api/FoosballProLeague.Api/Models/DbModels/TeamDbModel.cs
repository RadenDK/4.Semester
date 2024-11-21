namespace FoosballProLeague.Api.Models.DbModels
{
    public class TeamDbModel
    {
        public int Id { get; set; }
        public int Player1Id { get; set; }
        public int? Player2Id { get; set; }
    }
}
