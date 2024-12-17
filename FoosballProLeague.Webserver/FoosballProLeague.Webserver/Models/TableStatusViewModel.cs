namespace FoosballProLeague.Webserver.Models
{
    public class TableStatusViewModel
    {
        public string Side { get; set; } // "red" or "blue"
        public Dictionary<string, List<UserModel>> PendingUsers { get; set; } = new Dictionary<string, List<UserModel>>();
    }
}
