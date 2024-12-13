namespace FoosballProLeague.Webserver.Models;

public class TableLoginModel
{
    public int TableId { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Side { get; set; }
}