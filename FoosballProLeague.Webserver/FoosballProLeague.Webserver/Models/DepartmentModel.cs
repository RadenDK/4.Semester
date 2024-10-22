using System.Text.Json.Serialization;

namespace FoosballProLeague.Webserver.Models;

public class DepartmentModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("companyid")]
    public int? CompanyId { get; set; }
}