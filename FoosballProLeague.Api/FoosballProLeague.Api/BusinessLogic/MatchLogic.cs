using FoosballProLeague.Api.BusinessLogic.Interfaces;
using FoosballProLeague.Api.DatabaseAccess.Interfaces;
using FoosballProLeague.Api.Hubs;
using FoosballProLeague.Api.Models.FoosballModels;
using FoosballProLeague.Api.Models.RequestModels;
using Microsoft.AspNetCore.SignalR;

namespace FoosballProLeague.Api.BusinessLogic
{
    public class MatchLogic : IMatchLogic
    {
        private IMatchDatabaseAccessor _matchDatabaseAccessor;
        private IUserLogic _userLogic;
        private readonly IHubContext<HomepageHub> _hubContext;
        private ITeamDatabaseAccessor _teamDatabaseAccessor;
        private readonly Dictionary<int, PendingMatchTeamsModel> _pendingMatchTeams;

        public MatchLogic(IMatchDatabaseAccessor matchDatabaseAccessor, IHubContext<HomepageHub> hubContext, IUserLogic userLogic, ITeamDatabaseAccessor teamDatabaseAccessor)
        {
            _userLogic = userLogic;
            _matchDatabaseAccessor = matchDatabaseAccessor;
            _hubContext = hubContext;
            _pendingMatchTeams = new Dictionary<int, PendingMatchTeamsModel>();
            _teamDatabaseAccessor = teamDatabaseAccessor;
        }

        //Hjælpemetode til at hente alle kampe, bruges i GetActiveMatches til at loope igennem kampene, finde en uden endTime og tilføje holdene til listen
        public List<MatchModel> GetAllMatches()
        {
            return _matchDatabaseAccessor.GetAllMatches();
        }
       
        
        public MatchModel GetActiveMatch()
        {
            List<MatchModel> allMatches = _matchDatabaseAccessor.GetAllMatches();
            MatchModel activeMatch = new MatchModel();

            foreach (MatchModel match in allMatches)
            {
                if (match.EndTime == null)
                {
                    match.RedTeam = _teamDatabaseAccessor.GetTeamById(match.RedTeam.Id);
                    match.BlueTeam = _teamDatabaseAccessor.GetTeamById(match.BlueTeam.Id);
                    activeMatch = match;
                }
            }
            return activeMatch;
        }

        private TeamModel GetOrRegisterTeam(List<int?> userIds)
        {
            // Retrives the team if it already exists for the users
            TeamModel team = _teamDatabaseAccessor.GetTeamIdByUsers(userIds);

            if (team == null)
            {
                // If the users does not have a team then we register a new team
                team = _teamDatabaseAccessor.CreateTeam(userIds);
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
            MatchModel activeMatch = _matchDatabaseAccessor.GetActiveMatchByTableId(tableLoginRequest.TableId);

            // If there is no active match then we try to add the player to the pending match
            if (activeMatch == null)
            {
                // If there is no active then check if the table is in the pendingMatchTeams
                if (!_pendingMatchTeams.ContainsKey(tableLoginRequest.TableId))
                {
                    // If the table is not in the pendingMatchTeams then we add it to the dictionary
                    _pendingMatchTeams[tableLoginRequest.TableId] = new PendingMatchTeamsModel();
                }

                // If the table is in the pendingMatchTeams then we try to add the player to the pending match
                bool addedPlayerToPendingMatch = _pendingMatchTeams[tableLoginRequest.TableId].AddPlayer(tableLoginRequest.Side, tableLoginRequest.UserId);

                return addedPlayerToPendingMatch;
            }
            else
            {
                // If the match is active then we try to add the player to the active match instead of the pending tea

                // First we check if there is room on the active match team side
                bool roomAvailable = CheckForRoomOnActiveMatchTeamSide(activeMatch, tableLoginRequest);

                if (roomAvailable)
                {
                    // If there is room we will update the team to include the newly added player
                    // This should also invalidate the elo gain for the match
                    return AddPlayerToActiveMatchTeam(activeMatch, tableLoginRequest);
                }

                // There is no room on the active match team side then we return false
                else
                {
                    return false;
                }
            }
        }

        private bool CheckForRoomOnActiveMatchTeamSide(MatchModel activeMatch, TableLoginRequest tableLoginRequest)
        {
            TeamModel team = GetTeamBySide(activeMatch, tableLoginRequest.Side);

            bool roomAvailable = team.User2 == null;

            return roomAvailable;
        }

        private bool AddPlayerToActiveMatchTeam(MatchModel activeMatch, TableLoginRequest tableLoginRequest)
        {
            TeamModel currentTeam = GetTeamBySide(activeMatch, tableLoginRequest.Side);

            // This will return a teammodel for either the existing team or creating a new team
            TeamModel newTeam = GetOrRegisterTeam(new List<int?> { currentTeam.User1.Id, tableLoginRequest.UserId });

            // Updates the match model to have the new team model
            SetNewTeamBySide(activeMatch, newTeam, tableLoginRequest.Side);

            _matchDatabaseAccessor.UpdateMatchTeamIds(activeMatch);

            _matchDatabaseAccessor.UpdateValidEloMatch(activeMatch.Id, false);

            // If no expection happend we assume everything went as it should and return true
            return true;
        }

        // Helper method to retrieve team by side
        private TeamModel GetTeamBySide(MatchModel match, string side)
        {
            TeamModel relevantTeam = null;

            if (side == "red")
            {
                relevantTeam = match.RedTeam;
            }
            else if (side == "blue")
            {
                relevantTeam = match.BlueTeam;
            }

            return relevantTeam;
        }

        // Helper method to set update the matchmodel with a new team deping on the side
        private MatchModel SetNewTeamBySide(MatchModel match, TeamModel newTeam, string side)
        {
            if (side == "red")
            {
                match.RedTeam = newTeam;
            }
            else if (side == "blue")
            {
                match.BlueTeam = newTeam;
            }
            return match;
        }

        /*
         * StartMatch should take a tableId and try to start a match with the pending teams at the table.
         * It should create a match in the database and set the table to have an active match.
         * It should also remove the pending teams from memory.
         * If there is already an active match at the table, it should return false.
        */
        public bool StartMatch(int tableId)
        {

            // If there is an active match at the table then we return false because we cannot start a new match
            if (_matchDatabaseAccessor.GetActiveMatchByTableId(tableId) != null)
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

            bool validEloMatch = TeamsAreValidForRankedMatch(redTeam, blueTeam);

            int matchId = _matchDatabaseAccessor.CreateMatch(tableId, redTeam.Id, blueTeam.Id, validEloMatch);
            bool activeMatchWasSet = _matchDatabaseAccessor.UpdateTableActiveMatch(tableId, matchId);

            // Remove the pending teams from memory, since they are now in an active match
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
        
        private bool TeamsAreValidForRankedMatch(TeamModel redTeam, TeamModel blueTeam)
        {
            bool is1v1 = redTeam.User2 == null && blueTeam.User2 == null;
            bool is2v2 = redTeam.User2 != null && blueTeam.User2 != null;

            return is1v1 || is2v2;
        }

        public bool RegisterGoal(RegisterGoalRequest registerGoalRequest)
        {
            MatchModel activeMatch = _matchDatabaseAccessor.GetActiveMatchByTableId(registerGoalRequest.TableId);

            // If there is no active match at the table then we return false, since we cant register a goal
            if (activeMatch == null)
            {
                return false;
            }

            // If the registorGoalRequest side is red then we get the red team id else we get the blue team id
            TeamModel relevantTeam = GetTeamBySide(activeMatch, registerGoalRequest.Side);

            MatchLogModel matchLog = new MatchLogModel
            {
                MatchId = activeMatch.Id,
                Side = registerGoalRequest.Side,
                LogTime = DateTime.Now,
                TeamId = relevantTeam.Id
            };

            _matchDatabaseAccessor.CreateMatchLog(matchLog);

            UpdateScoreAndCheckMatchCompletion(activeMatch, registerGoalRequest);

            // If we didn't run into an exception then we assume everything went as it should and return true
            return true;
        }

        private bool UpdateScoreAndCheckMatchCompletion(MatchModel activeMatch, RegisterGoalRequest registerGoalRequest)
        {
            if (registerGoalRequest.Side == "red")
            {
                activeMatch.TeamRedScore++;
            }

            else if (registerGoalRequest.Side == "blue")
            {
                activeMatch.TeamBlueScore++;
            }

            _matchDatabaseAccessor.UpdateMatchScore(activeMatch);

            NotifyGoalsScored(registerGoalRequest).Wait();
            
            // If the score of either team is 10 then the match is over
            if (activeMatch.TeamRedScore == 10 || activeMatch.TeamBlueScore == 10)
            {
                // Set the active match at the table to null since the match is over
                _matchDatabaseAccessor.UpdateTableActiveMatch(registerGoalRequest.TableId, null);

                _matchDatabaseAccessor.EndMatch(activeMatch.Id);

                if (activeMatch.ValidEloMatch)
                {
                    _userLogic.UpdateTeamElo(activeMatch);
                }
            }

            NotifyMatchStartOrEnd(activeMatch.TableId, false).Wait();
            
            // If no expection happend we assume that everything went okay
            return true;
        }

        public void InterruptMatch(int tableId)
        {
            MatchModel activeMatch = _matchDatabaseAccessor.GetActiveMatchByTableId(tableId);

            if (activeMatch != null)
            {
                _matchDatabaseAccessor.UpdateTableActiveMatch(tableId, null);

                _matchDatabaseAccessor.EndMatch(activeMatch.Id);

                NotifyMatchStartOrEnd(tableId, false).Wait();
            }
        }
        
        // Method to send data to SignalR MatchHub when a match is starting or ending
        private async Task NotifyMatchStartOrEnd(int tableId, bool isMatchStart)
        {
            MatchModel match = _matchDatabaseAccessor.GetActiveMatchByTableId(tableId);

            if (match == null)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveMatchEnd", isMatchStart);
            }
            else
            {
                TeamModel redTeam = match.RedTeam;
                TeamModel blueTeam = match.BlueTeam;

                int redScore = match.TeamRedScore;
                int blueScore = match.TeamBlueScore;

                await _hubContext.Clients.All.SendAsync("ReceiveMatchStart", isMatchStart, redTeam, blueTeam, redScore, blueScore);
            }
        }
        
        // Method to send data to SignalR MatchHub when a goal is scored
        private async Task NotifyGoalsScored(RegisterGoalRequest registerGoalRequest)
        {
            MatchModel match = _matchDatabaseAccessor.GetActiveMatchByTableId(registerGoalRequest.TableId);

            TeamModel redTeam = match.RedTeam;
            TeamModel blueTeam = match.BlueTeam;

            int redScore = match.TeamRedScore;
            int blueScore = match.TeamBlueScore;

            await _hubContext.Clients.All.SendAsync("ReceiveGoalUpdate", redTeam, blueTeam, redScore, blueScore);
        }
    }
}

