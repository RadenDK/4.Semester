namespace FoosballProLeague.Api.Models.FoosballModels
{
    public class PendingMatchTeamsModel
    {
        public Dictionary<string, List<int>> Teams { get; private set; }

        public PendingMatchTeamsModel()
        {
            // Initialize teams for both sides (red and blue)
            Teams = new Dictionary<string, List<int>>
        {
            { "red", new List<int>() },
            { "blue", new List<int>() }
        };
        }

        public void AddPlayer(string side, int playerId)
        {
            if (Teams[side].Count < 2) // Limit to 2 players per side
            {
                Teams[side].Add(playerId);
            }
            else
            {
                throw new InvalidOperationException($"Cannot add more players to the {side} team.");
            }
        }

        public bool IsMatchReady()
        {
            return Teams["red"].Count > 0 && Teams["blue"].Count > 0;
        }
    }
}
