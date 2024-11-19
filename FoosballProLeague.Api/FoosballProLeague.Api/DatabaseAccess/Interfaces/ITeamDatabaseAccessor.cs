using FoosballProLeague.Api.Models.FoosballModels;
using Npgsql;

namespace FoosballProLeague.Api.DatabaseAccess.Interfaces
{
    public interface ITeamDatabaseAccessor
    {
        TeamModel GetTeamById(int teamId, NpgsqlConnection? connection = null);
        TeamModel GetTeamIdByUsers(List<int?> playerIds);
        TeamModel CreateTeam(List<int?> playerIds);
    }
}
