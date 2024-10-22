namespace FoosballProLeague.Api.Models.RequestModels
{
    public class TableLoginRequest
    {
        public int PlayerId { get; set; }
        public int TableId { get; set; }
        public string Side { get; set; }
    }
}
