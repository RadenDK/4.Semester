namespace FoosballProLeague.Api.Models.RequestModels
{
    public class TableLoginRequest
    {
        public int UserId { get; set; }
        public int TableId { get; set; }
        public string Side { get; set; }
    }
}
