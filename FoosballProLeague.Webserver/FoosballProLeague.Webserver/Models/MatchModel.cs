using System.Text.Json.Serialization;

namespace FoosballProLeague.Webserver.Models;

public class MatchModel
{
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("tableId")]
        public int TableId { get; set; }
        [JsonPropertyName("redTeam")]
        public TeamModel RedTeam { get; set; }
        [JsonPropertyName("blueTeam")]
        public TeamModel BlueTeam { get; set; }
        [JsonPropertyName("teamRedScore")]
        public int TeamRedScore { get; set; }
        [JsonPropertyName("teamBlueScore")]
        public int TeamBlueScore { get; set; }
        [JsonPropertyName("startTime")]
        public DateTime StartTime { get; set; }
        [JsonPropertyName("endTime")]
        public DateTime? EndTime { get; set; }
        [JsonPropertyName("validEloMatch")]   
        public bool ValidEloMatch { get; set; }
}