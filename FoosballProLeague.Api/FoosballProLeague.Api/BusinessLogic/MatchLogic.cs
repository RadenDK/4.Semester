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


        /*
         * LoginOnTable should take a login request which contains information about the player, table and side.
         * The Method should try to add players to a pending list waiting for a match to start.
         * It should also check if there is an active match at the table and if there is, try to add the player to the active match if there is room.
        */
        public bool LoginOnTable(TableLoginRequest tableLoginRequest)
        {
            bool playerSuccessfullyAdded = false;

            // First check if there is no active match at the table
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

            if (team != null && team.Player2Id == null)
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

        /*
         * StartMatch should take a tableId and try to start a match with the pending teams at the table.
         * It should create a match in the database and set the table to have an active match.
         * It should also remove the pending teams from memory.
         * If there is already an active match at the table, it should return false.
        */
        public bool StartMatch(int tableId)
        {
            bool matchStarted = false;

            int? activeMatchId = GetActiveMatchIdAtTable(tableId);

            if (activeMatchId != null)
            {
                return false; // There is already an active match at the table
            }
            else
            {
                int redTeamId = _matchDatabaseAccessor.RegisterTeam(_pendingMatchTeams[tableId].Teams["red"]);
                int blueTeamId = _matchDatabaseAccessor.RegisterTeam(_pendingMatchTeams[tableId].Teams["blue"]);

                // Create the match
                int matchId = _matchDatabaseAccessor.CreateMatch(tableId, redTeamId, blueTeamId);

                // Set the table active match
                bool activeMatchWasSet = _matchDatabaseAccessor.SetTableActiveMatch(tableId, matchId);

                // Remove the pending teams from memory
                _pendingMatchTeams.Remove(tableId);

                matchStarted = matchId != 0 && redTeamId != 0 && blueTeamId != 0 && activeMatchWasSet; // Check if all the operations were successful
            }

            return matchStarted;

        }

        private int? GetActiveMatchIdAtTable(int tableId)
        {
            int? activeMatchId = _matchDatabaseAccessor.GetActiveMatchIdByTableId(tableId);

            return activeMatchId;
        }

        public bool RegisterGoal(RegisterGoalRequest registerGoalRequest)
        {

            bool goalRegistered = false;

            int? matchId = _matchDatabaseAccessor.GetActiveMatchIdByTableId(registerGoalRequest.TableId);

            if (matchId == null)
            {
                return goalRegistered; // No active match at the table, so goal will not be registered
            }
            else
            {
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
                    goalRegistered = false; ; // LogGoal failed
                }

                UpdateScoreAndCheckMatchCompletion(matchId.Value, registerGoalRequest);

            }

            return goalRegistered;
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

        public void InterruptMatch(int tableId)
        {

            int? matchId = _matchDatabaseAccessor.GetActiveMatchIdByTableId(tableId);

            if (matchId == null)
            {
                return; // No active match at the table
            }
            else
            {
                _matchDatabaseAccessor.SetTableActiveMatch(tableId, null);
                _matchDatabaseAccessor.EndMatch(matchId.Value);
            }
        }
    }
}
