namespace FoosballProLeague.Api.Models;

public class UserModel
{
    public int UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int DepartmentId { get; set; }
    public int CompanyId { get; set; }
    public int Elo1v1 { get; set; }
    public int Elo2v2 { get; set; }
}