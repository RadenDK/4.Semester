using FoosballProLeague.Api.Models.FoosballModels;

namespace FoosballProLeague.Api.Models;

public class MatchHistoryModel
{
    public int Id { get; set; }
    public TeamModel RedTeam { get; set; }
    public TeamModel BlueTeam { get; set; }
    public int? RedTeamScore { get; set; }
    public int? BlueTeamScore { get; set; }
    public string EndTime { get; set; }
}