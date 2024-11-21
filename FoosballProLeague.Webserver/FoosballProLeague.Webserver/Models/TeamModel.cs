using System.Text.Json.Serialization;

namespace FoosballProLeague.Webserver.Models;

public class TeamModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("user1")]
    public UserModel User1 { get; set; }
    [JsonPropertyName("user2")]
    public UserModel? User2 { get; set; }
}