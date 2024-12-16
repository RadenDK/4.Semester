using FoosballProLeague.Api.Models.FoosballModels;
using Npgsql;

namespace FoosballProLeague.Api.DatabaseAccess.Interfaces
{
    public interface ITeamDatabaseAccessor
    {
        TeamModel GetTeamById(int teamId, NpgsqlConnection? connection = null);
        TeamModel GetTeamIdByUsers(IEnumerable<int> userIds);
        TeamModel CreateTeam(IEnumerable<int> userIds);
        bool RemovePendingUser(int userId);
    }
}
