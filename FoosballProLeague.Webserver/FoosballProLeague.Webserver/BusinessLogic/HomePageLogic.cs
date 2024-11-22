using FoosballProLeague.Webserver.Models;
using FoosballProLeague.Webserver.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FoosballProLeague.Webserver.BusinessLogic
{
    public class HomePageLogic : IHomePageLogic
    {
        private readonly IHomePageService _homePageService;
        private readonly ITokenLogic _tokenLogic;

        public HomePageLogic(IHomePageService homePageService, ITokenLogic tokenLogic)
        {
            _homePageService = homePageService;
            _tokenLogic = tokenLogic;
        }

       public async Task<List<UserModel>> GetLeaderboards(string mode, int pageNumber, int pageSize)
        {
            
            Dictionary<string, List<UserModel>> usersDictionary = await _homePageService.GetLeaderboards();
            if (usersDictionary.TryGetValue(mode, out List<UserModel> users))
            {
                return users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                throw new Exception($"Mode {mode} not found in the leaderboards.");
            }
        }

        public async Task<int> GetTotalUserCount(string mode)
        {
            Dictionary<string, List<UserModel>> userDict = await _homePageService.GetLeaderboards();
            if (userDict.TryGetValue(mode, out List<UserModel> users))
            {
                return users.Count;
            }
            else
            {
                throw new Exception($"Mode {mode} not found in the leaderboards.");
            }
        }

        public async Task<List<MatchHistoryModel>> GetMatchHistoryByUserId(int userId)
        {
            return await _homePageService.GetMatchHistoryByUserId(userId);
        }

        public async Task<HomePageViewModel> GetUsersAndMatchHistory(string mode, int pageNumber, int pageSize)
        {
            try
            {
                List<UserModel> users = await GetLeaderboards(mode, pageNumber, pageSize);
                UserModel user = GetUserFromJWT();
                List<MatchHistoryViewModel> matchHistory = null;

                if (user != null)
                {
                    try
                    {
                        List<MatchHistoryModel> matchHistoryModels = await GetMatchHistoryByUserId(user.Id);
                        if (matchHistoryModels != null)
                        {
                            matchHistory = matchHistoryModels
                            .OrderByDescending(m => DateTime.Parse(m.EndTime))
                            .Select(m => new MatchHistoryViewModel
                            {
                                RedTeamUser1 = m.RedTeam.User1.FirstName,
                                RedTeamUser2 = m.RedTeam.User2.FirstName,
                                BlueTeamUser1 = m.BlueTeam.User1.FirstName,
                                BlueTeamUser2 = m.BlueTeam.User2.FirstName,
                                RedTeamScore = m.RedTeamScore,
                                BlueTeamScore = m.BlueTeamScore,
                                TimeAgo = GetTimeAgo(m.EndTime)
                            }).ToList();
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException("An error occurred while retrieving match history.", ex);
                    }
                }

                MatchViewModel activeMatch = await GetActiveMatch();
                
                HomePageViewModel viewModel = new HomePageViewModel
                {
                    Users = users,
                    MatchHistory = matchHistory,
                    Mode = mode,
                    FullName = $"{user.FirstName} {user.LastName}",
                    TotalUserCount = users.Count,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    ActiveMatch = activeMatch ?? new MatchViewModel(),
                };
                return viewModel;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while retrieving leaderboard.", ex);
            }


        }

        private UserModel GetUserFromJWT()
        {
            JwtSecurityToken jsonToken = _tokenLogic.GetJWTFromCookie();

            Claim userIdClaim = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "UserId");
            Claim firstNameClaim = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "FirstName");
            Claim lastNameClaim = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "LastName");
            Claim elo1v1Claim = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "Elo1v1");
            Claim elo2v2Claim = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "Elo2v2");

            if (userIdClaim == null || firstNameClaim == null || lastNameClaim == null || elo1v1Claim == null || elo2v2Claim == null)
            {
                return null;
            }

            UserModel user = new UserModel
            {
                Id = int.Parse(userIdClaim.Value),
                FirstName = firstNameClaim.Value,
                LastName = lastNameClaim.Value,
                Elo1v1 = int.Parse(elo1v1Claim.Value),
                Elo2v2 = int.Parse(elo2v2Claim.Value)
            };

            return user;
        }

        public string GetTimeAgo(string endTime)
        {
            DateTime endDateTime = DateTime.Parse(endTime);
            TimeSpan timeSpan = DateTime.Now - endDateTime;

            if (timeSpan.TotalMinutes < 1)
            {
                return "Just now";
            }
            else if (timeSpan.TotalMinutes < 60)
            {
                return $"{(int)timeSpan.TotalMinutes} minutes ago";
            }
            else if (timeSpan.TotalHours < 24)
            {
                return $"{(int)timeSpan.TotalHours} hours ago";
            }
            else if (timeSpan.TotalDays < 30)
            {
                return $"{(int)timeSpan.TotalDays} days ago";
            }
            else if (timeSpan.TotalDays < 365)
            {
                return $"{(int)(timeSpan.TotalDays / 30)} months ago";
            }
            else
            {
                return $"{(int)(timeSpan.TotalDays / 365)} years ago";
            }
        }

        private async Task<MatchViewModel> GetActiveMatch()
        {
            MatchModel match = await _homePageService.GetActiveMatch();

            if (match == null)
            {
                return null;
            }
            return new MatchViewModel
            {
                RedTeamUser1 = $"{match.RedTeam.User1.FirstName} {match.RedTeam.User1.LastName}",
                RedTeamUser2 = $"{match.RedTeam?.User2?.FirstName} {match.RedTeam?.User2?.LastName}",
                BlueTeamUser1 = $"{match.BlueTeam.User1.FirstName} {match.BlueTeam.User1.LastName}",
                BlueTeamUser2 = $"{match.BlueTeam?.User2?.FirstName} {match.BlueTeam?.User2?.LastName}",
                RedTeamScore = match.TeamRedScore,
                BlueTeamScore = match.TeamBlueScore,
                StartTime = match.StartTime
            };
        }
    }
}
