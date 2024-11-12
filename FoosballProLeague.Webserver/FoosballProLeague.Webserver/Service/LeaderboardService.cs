using FoosballProLeague.Webserver.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;

namespace FoosballProLeague.Webserver.Service
{
    public class LeaderboardService : IHostedService
    {
        private HubConnection _hubConnection;
        private readonly ILogger<LeaderboardService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private const string API_URL = "http://localhost:5001"; // Configure this

        public event Action<List<UserModel>> OnLeaderboardUpdated;

        public LeaderboardService(ILogger<LeaderboardService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
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

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
                _logger.LogInformation("Disconnected from SignalR Hub");
            }
        }

        public async Task<List<UserModel>> GetSortedLeaderboard(string mode)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{API_URL}/leaderboard?mode={mode}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<UserModel>>(json);
            }
            else
            {
                _logger.LogError("Error fetching leaderboard: {0}", response.ReasonPhrase);
                return new List<UserModel>();
            }
        }
    }
}