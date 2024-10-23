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

        public MatchLogic(IMatchDatabaseAccessor matchDatabaseAccessor)
        {
            _matchDatabaseAccessor = matchDatabaseAccessor;
        }

        public bool LoginOnTable(TableLoginRequest tableLoginRequest)
        {
            bool playerSuccessfullyAdded = false;

            int? activateMatchId = GetActiveMatchIdAtTable(tableLoginRequest.TableId);
            if (activateMatchId == null) // there is no active match at the table
            {
                // If no pending match for the table, initialize it
                if (!_pendingMatchTeams.ContainsKey(tableLoginRequest.TableId))
                {
                    _pendingMatchTeams[tableLoginRequest.TableId] = new PendingMatchTeamsModel();
                }

                // Add player to the appropriate side. Will return false if player cannot be added
                playerSuccessfullyAdded = _pendingMatchTeams[tableLoginRequest.TableId].AddPlayer(tableLoginRequest.Side, tableLoginRequest.PlayerId);
            }
            else
            {

                if (CheckForRoomOnActiveMatchTeamSide(activateMatchId.Value, tableLoginRequest))
                {
                    playerSuccessfullyAdded = AddPlayerTooActiveMatchTeam(activateMatchId.Value, tableLoginRequest);

                    //TODO: Invalidate the ranking points of this match
                }
                else
                {
                    playerSuccessfullyAdded = false; // No room on the active match team side
                }
            }

            return playerSuccessfullyAdded;
        }

        private bool CheckForRoomOnActiveMatchTeamSide(int activateMatchId, TableLoginRequest tableLoginRequest)
        {
            bool roomOnTeam = false;

            MatchModel activeMatch = _matchDatabaseAccessor.GetMatchById(activateMatchId);

            TeamModel team = null;

            if (tableLoginRequest.Side == "red")
            {
                team = _matchDatabaseAccessor.GetTeamById(activeMatch.RedTeamId);
            }
            else if (tableLoginRequest.Side == "blue")
            {
                team = _matchDatabaseAccessor.GetTeamById(activeMatch.BlueTeamId);
            }

            if (team.Player2Id == null)
            {
                roomOnTeam = true;
            }

            return roomOnTeam;

        }

        private bool AddPlayerTooActiveMatchTeam(int matchId, TableLoginRequest tableLoginRequest)
        {
            MatchModel activeMatch = _matchDatabaseAccessor.GetMatchById(matchId);
            int? currentTeamId = null;

            if (tableLoginRequest.Side == "red")
            {
                currentTeamId = activeMatch.RedTeamId;
            }
            else if (tableLoginRequest.Side == "blue")
            {
                currentTeamId = activeMatch.BlueTeamId;
            }

            TeamModel currentTeam = _matchDatabaseAccessor.GetTeamById(currentTeamId.Value);

            List<int?> playerIds = new List<int?> { currentTeam.Player1Id, tableLoginRequest.PlayerId };

            int? newTeamId = _matchDatabaseAccessor.GetTeamIdByPlayers(playerIds);

            if (newTeamId == null)
            {
                newTeamId = _matchDatabaseAccessor.RegisterTeam(playerIds);
            }

            return _matchDatabaseAccessor.UpdateTeamId(matchId, tableLoginRequest.Side, newTeamId.Value);
        }


        public bool RegisterGoal(RegisterGoalRequest registerGoalRequest)
        {
            int? matchId = _matchDatabaseAccessor.GetActiveMatchIdByTableId(registerGoalRequest.TableId);

            if (matchId == null)
            {
                // Check if the table has pending teams before trying to access them
                if (!_pendingMatchTeams.ContainsKey(registerGoalRequest.TableId) || !_pendingMatchTeams[registerGoalRequest.TableId].IsMatchReady())
                {
                    // Return false or handle the case where no pending teams exist for the table
                    return false; // or return a BadRequest in the controller
                }

                // Create match if the teams are ready
                matchId = CreateMatchAndTeamsAndUpdateTableStatus(registerGoalRequest);
            }

            int teamId = _matchDatabaseAccessor.GetTeamIdByMatchId(matchId.Value, registerGoalRequest.Side);

            MatchLogModel matchLog = new MatchLogModel
            {
                MatchId = matchId.Value,
                Side = registerGoalRequest.Side,
                LogTime = DateTime.Now,
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
                match.TeamRedScore++;
            }
            else if (registerGoalRequest.Side == "blue")
            {
                match.TeamBlueScore++;
            }

            // Update score first
            _matchDatabaseAccessor.UpdateMatchScore(matchId, match.TeamRedScore, match.TeamBlueScore);

            // Check if the match is finished
            if (match.TeamRedScore == 10 || match.TeamBlueScore == 10)
            {
                _matchDatabaseAccessor.SetTableActiveMatch(registerGoalRequest.TableId, null);
                _matchDatabaseAccessor.EndMatch(matchId);
            }

            return true;
        }


        private int CreateMatchAndTeamsAndUpdateTableStatus(RegisterGoalRequest registerGoalRequest)
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


        private int? GetActiveMatchIdAtTable(int tableId)
        {
            int? activeMatchId = _matchDatabaseAccessor.GetActiveMatchIdByTableId(tableId);

            return activeMatchId;
        }
    }
}
