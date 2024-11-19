using System.Text.Json.Serialization;

namespace FoosballProLeague.Webserver.Models;

public class MatchHistoryModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("redTeam")]
    public TeamModel RedTeam { get; set; }
    [JsonPropertyName("blueTeam")]
    public TeamModel BlueTeam { get; set; }
    [JsonPropertyName("redTeamScore")]
    public int? RedTeamScore { get; set; }
    [JsonPropertyName("blueTeamScore")]
    public int? BlueTeamScore { get; set; }
    [JsonPropertyName("endTime")]
    public string EndTime { get; set; }
}