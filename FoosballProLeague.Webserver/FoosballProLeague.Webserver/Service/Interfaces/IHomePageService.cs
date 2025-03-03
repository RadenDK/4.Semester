﻿using FoosballProLeague.Webserver.Models;

namespace FoosballProLeague.Webserver.Service.Interfaces
{
    public interface IHomePageService
    {
        Task<Dictionary<string, List<UserModel>>> GetLeaderboards();
        public Task<List<MatchHistoryModel>> GetMatchHistoryByUserId(int userId);

        public Task<MatchModel> GetActiveMatch();
    }
}
