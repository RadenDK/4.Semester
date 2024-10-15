using System.ComponentModel.DataAnnotations;

namespace FoosballProLeague.Webserver.Models;

public class UserRegistrationModel
{
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", ErrorMessage = "Password must be at least 8 characters, include at least one upper case letter, one lower case letter, and one numeric digit.")]
    public string Password { get; set; }
}