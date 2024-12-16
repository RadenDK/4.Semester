namespace FoosballProLeague.Webserver.Models;

public class TableLoginViewModel
{
    public int TableId { get; set; }
    public List<TableLoginUserModel>? PendingUsers { get; set; }
    public string Email { get; set; }
    public string Side { get; set; }
}

public class TableLoginUserModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; }
    public string Side { get; set; }
}