using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.Models.FoosballModels;
using FoosballProLeague.Api.Models;
using System.Reflection.PortableExecutable;
using FoosballProLeague.Api.Models.RequestModels;
using System.Collections.Generic;
using FoosballProLeague.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using FoosballProLeague.Api.Models;


namespace FoosballProLeague.Api.BusinessLogic
{
    public class MatchLogic : IMatchLogic
    {
        private IMatchDatabaseAccessor _matchDatabaseAccessor;
        private IUserLogic _userLogic;
        private readonly IHubContext<HomepageHub> _hubContext;
        private static readonly Dictionary<int, PendingMatchTeamsModel> _pendingMatchTeams = new Dictionary<int, PendingMatchTeamsModel>();

        public MatchLogic(IMatchDatabaseAccessor matchDatabaseAccessor, IHubContext<HomepageHub> hubContext, IUserLogic userLogic)
        {
            _userLogic = userLogic;
            _matchDatabaseAccessor = matchDatabaseAccessor;
            _hubContext = hubContext;
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

        //Hjælpemetode til at hente alle kampe, bruges i GetActiveMatches til at loope igennem kampene, finde en uden endTime og tilføje holdene til listen
        public List<MatchModel> GetAllMatches()
        {
            return _matchDatabaseAccessor.GetAllMatches();
        }
       
        
        public List<MatchModel> GetActiveMatches()
        {
            List<MatchModel> allMatches = _matchDatabaseAccessor.GetAllMatches();
            List<MatchModel> activeMatches = new List<MatchModel>();

            foreach (MatchModel match in allMatches)
            {
                if (match.EndTime == null)
                {
                    match.RedTeam = _matchDatabaseAccessor.GetTeamById(match.RedTeamId);
                    match.BlueTeam = _matchDatabaseAccessor.GetTeamById(match.BlueTeamId);
                    activeMatches.Add(match);
                }
            }

            return activeMatches;
        }
        // Helper method to get or register a team by player IDs
        public TeamModel GetOrRegisterTeam(List<int?> userIds, int? existingTeamId = null)
        {
            int? teamId = _matchDatabaseAccessor.GetTeamIdByUsers(userIds);
            TeamModel team;
            if (teamId == null && existingTeamId == null)
            {
                int newTeamId = _matchDatabaseAccessor.RegisterTeam(userIds);
                team = _matchDatabaseAccessor.GetTeamById(newTeamId);
            }
            else
            {
                if (teamId != null)
                {
                    team = _matchDatabaseAccessor.GetTeamById(teamId.Value);
                }
                else
                {
                    team = _matchDatabaseAccessor.GetTeamById(existingTeamId.Value);
                    return team;
                }
            }
            return team;
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

                return _pendingMatchTeams[tableLoginRequest.TableId].AddPlayer(tableLoginRequest.Side, tableLoginRequest.UserId);
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

            return team != null && team.User2 == null;
        }

        private bool AddPlayerToActiveMatchTeam(int matchId, TableLoginRequest tableLoginRequest)
        {
            MatchModel activeMatch = _matchDatabaseAccessor.GetMatchById(matchId);
            TeamModel currentTeam = GetTeamBySide(activeMatch, tableLoginRequest.Side);

            int? existingTeamId = currentTeam.Id;
            
            List<int?> userIds = new List<int?> { currentTeam.User1.Id, tableLoginRequest.UserId };
            TeamModel newTeam = GetOrRegisterTeam(userIds, existingTeamId);

            if (_matchDatabaseAccessor.UpdateUserIdOnTeamByTeamId(existingTeamId, tableLoginRequest.UserId))
            {
                return _matchDatabaseAccessor.UpdateValidEloMatch(activeMatch.Id, false);
            }

            return false;
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
            {
                return false;
            }

            // If the table is not in the pendingMatchTeams then no one has tried to login into the table.
            // Therefore we know that we cannot start a match and return false
            if (!_pendingMatchTeams.ContainsKey(tableId) || !_pendingMatchTeams[tableId].IsMatchReady())
            {
                return false;
            }

            TeamModel redTeam = GetOrRegisterTeam(_pendingMatchTeams[tableId].Teams["red"]);
            TeamModel blueTeam = GetOrRegisterTeam(_pendingMatchTeams[tableId].Teams["blue"]);

            bool validEloMatch = IsValidEloMatch(redTeam, blueTeam);
            
            int matchId = _matchDatabaseAccessor.CreateMatch(tableId, redTeam.Id, blueTeam.Id, validEloMatch);
            bool activeMatchWasSet = _matchDatabaseAccessor.SetTableActiveMatch(tableId, matchId);



            _pendingMatchTeams.Remove(tableId);

            if (matchId != 0 && redTeam.Id != 0 && blueTeam.Id != 0 && activeMatchWasSet)
            {
                NotifyMatchStartOrEnd(tableId, true).Wait();
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsValidEloMatch(TeamModel redTeam, TeamModel blueTeam)
        {
            bool is1v1 = redTeam.User2 == null && blueTeam.User2 == null;
            bool is2v2 = redTeam.User2 != null && blueTeam.User2 != null;
            
            return is1v1 || is2v2;
        }

        public bool RegisterGoal(RegisterGoalRequest registerGoalRequest)
        {
            int? matchId = _matchDatabaseAccessor.GetActiveMatchIdByTableId(registerGoalRequest.TableId);
            if (matchId == null)
            {
                return false;
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
                return false;
            }

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
            NotifyGoalsScored(registerGoalRequest).Wait();

            if (match.TeamRedScore == 10 || match.TeamBlueScore == 10)
            {
                _matchDatabaseAccessor.SetTableActiveMatch(registerGoalRequest.TableId, null);
                _matchDatabaseAccessor.EndMatch(matchId);

                TeamModel redTeam = _matchDatabaseAccessor.GetTeamById(match.RedTeamId);
                TeamModel blueTeam = _matchDatabaseAccessor.GetTeamById(match.BlueTeamId);

                bool redTeamWon = match.TeamRedScore == 10;
                bool is1v1 = redTeam.User2 == null && blueTeam.User2 == null;
                bool validEloMatch = match.ValidEloMatch;

                if((is1v1 && (redTeam.User2 == null && blueTeam.User2 == null)) || (!is1v1 && (redTeam.User2 != null && blueTeam.User2 != null)) && validEloMatch)
                {
                    NotifyMatchStartOrEnd(registerGoalRequest.TableId, false).Wait();
                    _userLogic.UpdateTeamElo(redTeam, blueTeam, redTeamWon, is1v1);
                }
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
                NotifyMatchStartOrEnd(tableId, false).Wait();
            }
        }

        // Method to send data to SignalR MatchHub when a goal is scored
        public async Task NotifyGoalsScored(RegisterGoalRequest registerGoalRequest)
        {
            int? matchId = _matchDatabaseAccessor.GetActiveMatchIdByTableId(registerGoalRequest.TableId);

            int redTeamId = _matchDatabaseAccessor.GetTeamIdByMatchId(matchId.Value, "red");
            int blueTeamId = _matchDatabaseAccessor.GetTeamIdByMatchId(matchId.Value, "blue");

            TeamModel redTeam = _matchDatabaseAccessor.GetTeamById(redTeamId);
            TeamModel blueTeam = _matchDatabaseAccessor.GetTeamById(blueTeamId);

            MatchModel match = _matchDatabaseAccessor.GetMatchById(matchId.Value);
            int redScore = match.TeamRedScore;
            int blueScore = match.TeamBlueScore;

            await _hubContext.Clients.All.SendAsync("ReceiveGoalUpdate", redTeam, blueTeam, redScore, blueScore);
        }

        // Method to send data to SignalR MatchHub when a match is starting or ending
        public async Task NotifyMatchStartOrEnd(int tableId, bool isMatchStart)
        {
            int? matchId = _matchDatabaseAccessor.GetActiveMatchIdByTableId(tableId);

            if (matchId == null)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveMatchEnd", isMatchStart);
            }
            else
            {
                int redTeamId = _matchDatabaseAccessor.GetTeamIdByMatchId(matchId.Value, "red");
                int blueTeamId = _matchDatabaseAccessor.GetTeamIdByMatchId(matchId.Value, "blue");

                TeamModel redTeam = _matchDatabaseAccessor.GetTeamById(redTeamId);
                TeamModel blueTeam = _matchDatabaseAccessor.GetTeamById(blueTeamId);

                MatchModel match = _matchDatabaseAccessor.GetMatchById(matchId.Value);
                int redScore = match.TeamRedScore;
                int blueScore = match.TeamBlueScore;

                await _hubContext.Clients.All.SendAsync("ReceiveMatchStart", isMatchStart, redTeam, blueTeam, redScore, blueScore);
            }
        }
    }
}
