namespace FoosballProLeague.Api.Models.RequestModels
{
    public class TableLoginRequest
    {
        public string? Email { get; set; }
        public int TableId { get; set; }
        public string? Side { get; set; }
    }
}
