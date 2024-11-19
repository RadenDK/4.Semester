using Npgsql;

namespace FoosballProLeague.Api.DatabaseAccess
{
    public class DatabaseAccessor
    {

        private readonly string _connectionString;
        public DatabaseAccessor(IConfiguration configuration)
        {
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

            _connectionString = configuration.GetConnectionString("DatabaseConnection");
        }

        public NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

    }
}
