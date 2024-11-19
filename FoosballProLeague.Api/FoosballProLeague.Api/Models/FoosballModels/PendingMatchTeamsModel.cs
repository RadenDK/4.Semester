
namespace FoosballProLeague.Api.Models.FoosballModels
{
    public class PendingMatchTeamsModel
    {
        public Dictionary<string, List<int?>> Teams { get; private set; }

        public PendingMatchTeamsModel()
        {
            // Initialize teams for both sides (red and blue) with two null values
            Teams = new Dictionary<string, List<int?>>
            {
                { "red", new List<int?> { null, null } },
                { "blue", new List<int?> { null, null } }
            };
        }

        public bool AddPlayer(string side, int playerId)
        {
            // Replace the first null value with the player ID
            for (int i = 0; i < Teams[side].Count; i++)
            {
                if (Teams[side][i] == null)
                {
                    Teams[side][i] = playerId;
                    return true;
                }
            }
            // If no null spots are left, return false

            return false;

        }

        public bool IsMatchReady()
        {
            // Check if both teams have at least one player
            return Teams["red"].Any(p => p != null) && Teams["blue"].Any(p => p != null); // Check if both teams have at least one player
        }
    }
}
