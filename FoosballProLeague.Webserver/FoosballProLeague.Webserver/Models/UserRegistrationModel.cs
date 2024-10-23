using System.ComponentModel.DataAnnotations;

namespace FoosballProLeague.Webserver.Models;

public class UserRegistrationModel
{
    [Required(ErrorMessage = "First name is required")]
    public string FirstName { get; set; }
    [Required(ErrorMessage = "Last name is required")]
    public string LastName { get; set; }
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Password is required")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", ErrorMessage = "Password must be at least 8 characters, include at least one upper case letter, one lower case letter, and one numeric digit.")]
    public string Password { get; set; }
    
    [Required(ErrorMessage = "Field is required")]
    [Compare("Password", ErrorMessage = "Passwords are not equal")]
    public string ConfirmPassword { get; set; }
    
    [Required(ErrorMessage = "Company is required")]
    public int? CompanyId { get; set; }
    
    [Required(ErrorMessage = "Department is required")]
    public int? DepartmentId { get; set; }

    public int Elo1v1 { get; set; } = 0;

    public int Elo2v2 { get; set; } = 0;
}