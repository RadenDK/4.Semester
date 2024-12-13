namespace FoosballProLeague.Api.Models.RequestModels
{
    public class TableLoginRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        public int TableId { get; set; }
        public string Side { get; set; }
    }
}
