using FoosballProLeague.Webserver.Models;

namespace FoosballProLeague.Webserver.Service;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class LeaderboardService : IHostedService
{
    private HubConnection _hubConnection;
    private readonly ILogger<LeaderboardService> _logger;
    private const string API_URL = "http://localhost:5001"; // Configure this
    
    public event Action<List<UserModel>> OnLeaderboardUpdated;

    public LeaderboardService(ILogger<LeaderboardService> logger)
    {
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        string hubUrl = $"{API_URL.TrimEnd('/')}/leaderboardHub";

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<List<UserModel>>("ReceiveLeaderboardUpdate", (leaderboard) =>
        {
            OnLeaderboardUpdated?.Invoke(leaderboard);
        });

        try
        {
            await _hubConnection.StartAsync(cancellationToken);
            _logger.LogInformation("Connected to SignalR Hub at {0}", hubUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to SignalR Hub at {0}", hubUrl);
        }
    }

    // Add the StopAsync method to implement IHostedService
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
            _logger.LogInformation("Disconnected from SignalR Hub");
        }
    }
}
