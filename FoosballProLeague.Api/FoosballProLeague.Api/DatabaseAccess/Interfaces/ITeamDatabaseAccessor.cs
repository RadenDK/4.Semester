using FoosballProLeague.Api.Models.FoosballModels;
using Npgsql;

namespace FoosballProLeague.Api.DatabaseAccess.Interfaces
{
    public interface ITeamDatabaseAccessor
    {
        TeamModel GetTeamById(NpgsqlConnection connection, int teamId);
        TeamModel GetTeamIdByUsers(List<int?> playerIds);
        TeamModel CreateTeam(List<int?> playerIds);
    }
}
