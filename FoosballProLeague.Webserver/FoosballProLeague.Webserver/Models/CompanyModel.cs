using System.Text.Json.Serialization;

namespace FoosballProLeague.Webserver.Models;

public class CompanyModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
}