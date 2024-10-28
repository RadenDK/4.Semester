using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.Models.FoosballModels;
using FoosballProLeague.Api.Models.RequestModels;
using System.Collections.Generic;

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

        // Helper method to retrieve team by side
        private TeamModel GetTeamBySide(MatchModel match, string side)
        {
            int teamId;
            if (side == "red")
            {
                teamId = match.RedTeamId;
            }
            else
            {
                teamId = match.BlueTeamId;
            }

            return _matchDatabaseAccessor.GetTeamById(teamId);
        }

        // Helper method to get or register a team by player IDs
        private int? GetOrRegisterTeam(List<int?> playerIds)
        {
            int? teamId = _matchDatabaseAccessor.GetTeamIdByPlayers(playerIds);
            if (teamId == null)
            {
                teamId = _matchDatabaseAccessor.RegisterTeam(playerIds);
            }
            return teamId;
        }

        /*
         * LoginOnTable should take a login request which contains information about the player, table and side.
         * The Method should try to add players to a pending list waiting for a match to start.
         * It should also check if there is an active match at the table and if there is, try to add the player to the active match if there is room.
        */
        public bool LoginOnTable(TableLoginRequest tableLoginRequest)
        {
            int? activeMatchId = _matchDatabaseAccessor.GetActiveMatchIdByTableId(tableLoginRequest.TableId);

            if (activeMatchId == null)
            {
                if (!_pendingMatchTeams.ContainsKey(tableLoginRequest.TableId))
                {
                    _pendingMatchTeams[tableLoginRequest.TableId] = new PendingMatchTeamsModel();
                }

                return _pendingMatchTeams[tableLoginRequest.TableId].AddPlayer(tableLoginRequest.Side, tableLoginRequest.PlayerId);
            }

            bool roomAvailable = CheckForRoomOnActiveMatchTeamSide(activeMatchId.Value, tableLoginRequest);
            if (roomAvailable)
            {
                return AddPlayerToActiveMatchTeam(activeMatchId.Value, tableLoginRequest);
            }

            return false;
        }

        private bool CheckForRoomOnActiveMatchTeamSide(int activeMatchId, TableLoginRequest tableLoginRequest)
        {
            MatchModel activeMatch = _matchDatabaseAccessor.GetMatchById(activeMatchId);
            TeamModel team = GetTeamBySide(activeMatch, tableLoginRequest.Side);
            return team != null && team.Player2Id == null;
        }

        private bool AddPlayerToActiveMatchTeam(int matchId, TableLoginRequest tableLoginRequest)
        {
            MatchModel activeMatch = _matchDatabaseAccessor.GetMatchById(matchId);
            TeamModel currentTeam = GetTeamBySide(activeMatch, tableLoginRequest.Side);
            List<int?> playerIds = new List<int?> { currentTeam.Player1Id, tableLoginRequest.PlayerId };
            int? newTeamId = GetOrRegisterTeam(playerIds);
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
            if (_matchDatabaseAccessor.GetActiveMatchIdByTableId(tableId) != null)
                return false;

            int? redTeamId = GetOrRegisterTeam(_pendingMatchTeams[tableId].Teams["red"]);
            int? blueTeamId = GetOrRegisterTeam(_pendingMatchTeams[tableId].Teams["blue"]);

            int matchId = _matchDatabaseAccessor.CreateMatch(tableId, redTeamId.Value, blueTeamId.Value);
            bool activeMatchWasSet = _matchDatabaseAccessor.SetTableActiveMatch(tableId, matchId);

            _pendingMatchTeams.Remove(tableId);

            return matchId != 0 && redTeamId != 0 && blueTeamId != 0 && activeMatchWasSet;
        }

        public bool RegisterGoal(RegisterGoalRequest registerGoalRequest)
        {
            int? matchId = _matchDatabaseAccessor.GetActiveMatchIdByTableId(registerGoalRequest.TableId);
            if (matchId == null) return false;

            int teamId = _matchDatabaseAccessor.GetTeamIdByMatchId(matchId.Value, registerGoalRequest.Side);
            MatchLogModel matchLog = new MatchLogModel
            {
                MatchId = matchId.Value,
                Side = registerGoalRequest.Side,
                LogTime = DateTime.Now,
                TeamId = teamId
            };

            if (!_matchDatabaseAccessor.LogGoal(matchLog)) return false;

            UpdateScoreAndCheckMatchCompletion(matchId.Value, registerGoalRequest);
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

            _matchDatabaseAccessor.UpdateMatchScore(matchId, match.TeamRedScore, match.TeamBlueScore);

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
            if (matchId != null)
            {
                _matchDatabaseAccessor.SetTableActiveMatch(tableId, null);
                _matchDatabaseAccessor.EndMatch(matchId.Value);
            }
        }
    }
}
