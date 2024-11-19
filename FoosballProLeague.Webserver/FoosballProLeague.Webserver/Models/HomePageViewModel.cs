namespace FoosballProLeague.Webserver.Models;

public class HomePageViewModel
{
    public List<UserModel> Users { get; set; }
    public List<MatchHistoryViewModel> MatchHistory { get; set; }
    public string Mode { get; set; }
}

public class MatchHistoryViewModel
{
    public string RedTeamUser1 { get; set; }
    public string? RedTeamUser2 { get; set; }
    public string BlueTeamUser1 { get; set; }
    public string? BlueTeamUser2 { get; set; }
    public int? RedTeamScore { get; set; }
    public int? BlueTeamScore { get; set; }
    public string TimeAgo { get; set; }
}