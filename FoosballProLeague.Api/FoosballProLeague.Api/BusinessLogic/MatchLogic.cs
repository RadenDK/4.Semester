using FoosballProLeague.Api.BusinessLogic.Interfaces;
using FoosballProLeague.Api.DatabaseAccess.Interfaces;
using FoosballProLeague.Api.Models;
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
        private readonly IHubContext<HomepageHub> _homepageHub;
        private readonly IHubContext<TableLoginHub> _tableLoginHub;
        private ITeamDatabaseAccessor _teamDatabaseAccessor;

        public MatchLogic(IMatchDatabaseAccessor matchDatabaseAccessor, IHubContext<HomepageHub> homepageHub, IHubContext<TableLoginHub> tableLoginHub, IUserLogic userLogic, ITeamDatabaseAccessor teamDatabaseAccessor)
        {
            _userLogic = userLogic;
            _matchDatabaseAccessor = matchDatabaseAccessor;
            _homepageHub = homepageHub;
            _tableLoginHub = tableLoginHub;
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
            MatchModel activeMatch = _matchDatabaseAccessor.GetActiveMatchByTableId(1);

            return activeMatch;
        }

        private TeamModel GetOrRegisterTeam(IEnumerable<int> userIds)
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
            // Check if the userId exists in the database
            UserModel user = _userLogic.GetUserByEmail(tableLoginRequest.Email);
            if (user == null)
            {
                return false; // Invalid user, cannot proceed
            }

            // Check if there is an active match
            MatchModel activeMatch = _matchDatabaseAccessor.GetActiveMatchByTableId(tableLoginRequest.TableId);

            if (activeMatch == null)
            {
                // Handle pending login if no active match
                return HandlePendingLogin(tableLoginRequest);
            }
            else
            {
                // Handle active match if it exists
                return HandleActiveMatchLogin(activeMatch, tableLoginRequest);
            }
        }

        private bool HandlePendingLogin(TableLoginRequest tableLoginRequest)
        {
            // Fetch all pending logins for the table
            List<TableLoginRequest> pendingLogins = _matchDatabaseAccessor.GetPendingLoginsByTableId(tableLoginRequest.TableId);

            // Check if the user already has a login request for this table
            TableLoginRequest existingRequest = pendingLogins.FirstOrDefault(p => p.UserId == tableLoginRequest.UserId);

            if (existingRequest != null)
            {
                // Invalidate the existing request by marking it as "removed"
                _matchDatabaseAccessor.UpdateLoginRequestStatus(existingRequest.Id, "removed");
            }

            // Check if there is room on the specified side
            if (IsSideFull(pendingLogins, tableLoginRequest.Side))
            {
                return false; // No room on the side
            }

            // Add the new login request
            return _matchDatabaseAccessor.AddLoginRequest(tableLoginRequest);
        }


        private bool IsSideFull(List<TableLoginRequest> pendingLogins, string side)
        {
            int sideCount = pendingLogins.Count(p => p.Side == side);
            return sideCount >= 2; // Side is full if it has 2 players
        }

        private bool HandleActiveMatchLogin(MatchModel activeMatch, TableLoginRequest tableLoginRequest)
        {
            // Check if tableLoginRequest contains Player already in match
            if (CheckPlayerIsNotInActiveMatch(tableLoginRequest.UserId, activeMatch) == false)
            {
                return false;   
            }

            // Check if there is room on the active match team side
            if (!CheckForRoomOnActiveMatchTeamSide(activeMatch, tableLoginRequest))
            {
                return false; // No room on the active match team
            }

            // Add the player to the active match team
            return AddPlayerToActiveMatchTeam(activeMatch, tableLoginRequest);
        }

        private bool CheckPlayerIsNotInActiveMatch(int userId, MatchModel activeMatch)
        {
            // Check if the player is part of the red or blue team
            bool isInRedTeam = activeMatch.RedTeam.User1.Id == userId ||
                               (activeMatch.RedTeam.User2?.Id == userId); 

            bool isInBlueTeam = activeMatch.BlueTeam.User1.Id == userId ||
                                (activeMatch.BlueTeam.User2?.Id == userId);

            // Return true if the player is not in either team
            return !(isInRedTeam || isInBlueTeam);
        }

        private bool CheckForRoomOnActiveMatchTeamSide(MatchModel activeMatch, TableLoginRequest tableLoginRequest)
        {
            TeamModel team = GetTeamBySide(activeMatch, tableLoginRequest.Side);

            bool roomAvailable = team.User2 == null;

            return roomAvailable;
        }

        private bool AddPlayerToActiveMatchTeam(MatchModel activeMatch, TableLoginRequest tableLoginRequest, int userId)
        {
            TeamModel currentTeam = GetTeamBySide(activeMatch, tableLoginRequest.Side);

            // This will return a teammodel for either the existing team or creating a new team
            List<int> newTeamUserIds = new List<int> { currentTeam.User1.Id, tableLoginRequest.UserId };
            TeamModel newTeam = GetOrRegisterTeam(newTeamUserIds);

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
         * It should also update the status of pending teams in the database.
         * If there is already an active match at the table, it should return false.
        */
        public bool StartMatch(int tableId)
        {
            // Check if there is already an active match at the table.
            if (_matchDatabaseAccessor.GetActiveMatchByTableId(tableId) != null)
            {
                return false; // Cannot start a new match if one is active
            }

            // Fetch all pending login requests for the table
            IEnumerable<TableLoginRequest> pendingLogins = _matchDatabaseAccessor.GetPendingLoginsByTableId(tableId);

            // Separate pending users into red and blue sides
            IEnumerable<int> redSidePendingUsers = pendingLogins.Where(p => p.Side == "red").Select(p => p.UserId).ToList();
            IEnumerable<int> blueSidePendingUsers = pendingLogins.Where(p => p.Side == "blue").Select(p => p.UserId).ToList();

            // Ensure both sides have at least one player
            if (redSidePendingUsers.Count() == 0 || blueSidePendingUsers.Count() == 0)
            {
                return false; // Both sides must have players
            }

            // Check if the match qualifies as a valid Elo match (1v1 or 2v2)
            bool validEloMatch = redSidePendingUsers.Count() == blueSidePendingUsers.Count();

            // Create or retrieve teams for both sides
            TeamModel redTeam = GetOrRegisterTeam(redSidePendingUsers);
            TeamModel blueTeam = GetOrRegisterTeam(blueSidePendingUsers);

            // Create a new match and set it as the active match for the table
            int matchId = _matchDatabaseAccessor.CreateMatch(tableId, redTeam.Id, blueTeam.Id, validEloMatch);
            bool activeMatchWasSet = _matchDatabaseAccessor.UpdateTableActiveMatch(tableId, matchId);

            // Notify players that the match has started
            NotifyMatchStartOrEnd(tableId, true).Wait();

            // Mark all pending login requests as "solved"
            foreach (TableLoginRequest loginRequest in pendingLogins)
            {
                _matchDatabaseAccessor.UpdateLoginRequestStatus(loginRequest.Id, "solved");
            }

            return true; // Successfully started the match
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

                NotifyMatchStartOrEnd(activeMatch.TableId, false).Wait();

                if (activeMatch.ValidEloMatch)
                {
                    _userLogic.UpdateTeamElo(activeMatch);
                }
            }

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
                if (_homepageHub.Clients != null)
                {
                    await _homepageHub.Clients.All.SendAsync("ReceiveMatchEnd", isMatchStart);
                }
            }
            else
            {
                TeamModel redTeam = match.RedTeam;
                TeamModel blueTeam = match.BlueTeam;

                int redScore = match.TeamRedScore;
                int blueScore = match.TeamBlueScore;

                if (_homepageHub.Clients != null)
                {
                    await _homepageHub.Clients.All.SendAsync("ReceiveMatchStart", isMatchStart, redTeam, blueTeam, redScore, blueScore);
                }
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

            if (_homepageHub.Clients != null)
            {
                await _homepageHub.Clients.All.SendAsync("ReceiveGoalUpdate", redTeam, blueTeam, redScore, blueScore);
            }
        }

        private async Task NotifyTableLogin(UserModel user)
        {
            if (_tableLoginHub.Clients != null)
            {
                await _tableLoginHub.Clients.All.SendAsync("ReceiveTableLogin", user);
            }
        }

        public void ClearPendingTeamsCache()
        {
            IEnumerable<TableLoginRequest> pendingRequests = _matchDatabaseAccessor.GetPendingLoginsByTableId(1);
            foreach(TableLoginRequest pendingRequest in pendingRequests)
            {
                _matchDatabaseAccessor.UpdateLoginRequestStatus(pendingRequest.Id, "removed");
            }
        }
    }
}

