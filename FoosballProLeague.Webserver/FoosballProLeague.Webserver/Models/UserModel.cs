using System.Text.Json.Serialization;

namespace FoosballProLeague.Webserver.Models;

public class UserModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; }
    [JsonPropertyName("lastName")]
    public string LastName { get; set; }
    [JsonPropertyName("email")]
    public string? Email { get; set; }
    [JsonPropertyName("password")]
    public string? Password { get; set; }
    [JsonPropertyName("elo1v1")]
    public int Elo1v1 { get; set; }
    [JsonPropertyName("elo2v2")]
    public int Elo2v2 { get; set; }

}