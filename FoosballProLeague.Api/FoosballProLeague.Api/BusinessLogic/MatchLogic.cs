using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.Models.FoosballModels;
using System.Reflection.PortableExecutable;
using FoosballProLeague.Api.Models.RequestModels;

namespace FoosballProLeague.Api.BusinessLogic
{
    public class MatchLogic : IMatchLogic
    {

        private IMatchDatabaseAccessor _matchDatabaseAccessor;

        private readonly Dictionary<int, PendingMatchTeamsModel> _pendingMatchTeams = new Dictionary<int, PendingMatchTeamsModel>();

        public bool LoginOnTable(TableLoginRequest tableLoginRequest)
        {
            if (GetActiveMatchAtTable(tableLoginRequest.TableId) == null)
            {
                // If no pending match for the table, initialize it
                if (!_pendingMatchTeams.ContainsKey(tableLoginRequest.TableId))
                {
                    _pendingMatchTeams[tableLoginRequest.TableId] = new PendingMatchTeamsModel();
                }

                // Add player to the appropriate side
                try
                {
                    _pendingMatchTeams[tableLoginRequest.TableId].AddPlayer(tableLoginRequest.Side, tableLoginRequest.PlayerId);
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }
            else
            {
                return false; // Reject if a match is already active
            }

            return true;
        }


        public bool RegisterGoal(RegisterGoalRequest registerGoalRequest)
        {
            int? matchId = _matchDatabaseAccessor.GetActiveMatchByTableId(registerGoalRequest.TableId);

            if (matchId == null)
            {
                matchId = CreateMatchAndUpdateTableStatus(registerGoalRequest);
            }

            int teamId = _matchDatabaseAccessor.GetTeamIdByMatchId(matchId.Value, registerGoalRequest.Side);

            MatchLogModel matchLog = new MatchLogModel
            {
                MatchId = matchId.Value,
                Side = registerGoalRequest.Side,
                Log_time = DateTime.Now,
                TeamId = teamId
            };

            if (!_matchDatabaseAccessor.LogGoal(matchLog))
            {
                return false; // LogGoal failed
            }

            if (!UpdateScoreAndCheckMatchCompletion(matchId.Value, registerGoalRequest))
            {
                return false; // Score update failed or match completion failed
            }

            return true;
        }


        private bool UpdateScoreAndCheckMatchCompletion(int matchId, RegisterGoalRequest registerGoalRequest)
        {
            MatchModel match = _matchDatabaseAccessor.GetMatchById(matchId);

            if (registerGoalRequest.Side == "red")
            {
                match.RedScore++;
            }
            else if (registerGoalRequest.Side == "blue")
            {
                match.BlueScore++;
            }

            // Update score first
            _matchDatabaseAccessor.UpdateMatchScore(matchId, match.RedScore, match.BlueScore);

            // Check if the match is finished
            if (match.RedScore == 10 || match.BlueScore == 10)
            {
                _matchDatabaseAccessor.SetTableActiveMatch(registerGoalRequest.TableId, null);
                _matchDatabaseAccessor.EndMatch(matchId);
            }

            return true;
        }


        private int CreateMatchAndUpdateTableStatus(RegisterGoalRequest registerGoalRequest)
        {
            int matchId;

            // Get team IDs for both sides
            int? redTeamId = _matchDatabaseAccessor.GetTeamIdByPlayers(_pendingMatchTeams[registerGoalRequest.TableId].Teams["red"]);
            int? blueTeamId = _matchDatabaseAccessor.GetTeamIdByPlayers(_pendingMatchTeams[registerGoalRequest.TableId].Teams["blue"]);

            // Register teams if they don't exist
            if (redTeamId == null)
            {
                redTeamId = _matchDatabaseAccessor.RegisterTeam(_pendingMatchTeams[registerGoalRequest.TableId].Teams["red"]);
            }
            if (blueTeamId == null)
            {
                blueTeamId = _matchDatabaseAccessor.RegisterTeam(_pendingMatchTeams[registerGoalRequest.TableId].Teams["blue"]);
            }

            // Create the match
            matchId = _matchDatabaseAccessor.CreateMatch(registerGoalRequest.TableId, redTeamId.Value, blueTeamId.Value);

            // Set the table active match
            _matchDatabaseAccessor.SetTableActiveMatch(registerGoalRequest.TableId, matchId);

            // Remove the pending teams from memory
            _pendingMatchTeams.Remove(registerGoalRequest.TableId);

            return matchId;
        }


        private bool GetActiveMatchAtTable(int tableId)
        {
            int? activeMatchId = _matchDatabaseAccessor.GetActiveMatchByTableId(tableId);

            return activeMatchId != null;
        }
    }
}
