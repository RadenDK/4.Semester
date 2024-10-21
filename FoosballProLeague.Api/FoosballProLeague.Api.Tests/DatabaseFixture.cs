using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Xunit;

namespace FoosballProLeague.Api.Tests
{
    public class DatabaseFixture : IAsyncLifetime, IDisposable
    {
        public string TestDatabaseConnection { get; private set; }

        private IConfiguration _configuration;

        public DatabaseFixture()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            TestDatabaseConnection = _configuration.GetConnectionString("DatabaseConnection");
        }

        public IConfiguration GetConfiguration()
        {
            return _configuration;
        }

        public void InsertData(string query)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(TestDatabaseConnection))
            {
                connection.Open();
                connection.Execute(query);
            }
        }

        public IEnumerable<T> ReadData<T>(string query)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(TestDatabaseConnection))
            {
                connection.Open();
                return connection.Query<T>(query).ToList();
            }
        }

        public void ClearDatabase()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(TestDatabaseConnection))
            {
                connection.Open();
                string truncateQuery = @"
                TRUNCATE match_logs, teams, foosball_matches, users, foosball_tables, departments, companies
                RESTART IDENTITY CASCADE;
            ";
                connection.Execute(truncateQuery);
            }
        }

        // This will run before each test to clean the database
        public Task InitializeAsync()
        {
            ClearDatabase();
            return Task.CompletedTask;
        }

        // This will run after each test for any further cleanup
        public Task DisposeAsync()
        {
            // Additional cleanup if needed
            return Task.CompletedTask;
        }

        // Optional: cleanup after all tests (already handled by IAsyncLifetime, can be removed)
        public void Dispose()
        {
            // Clear database when the fixture is disposed (after all tests have run)
            ClearDatabase();
        }
    }
}
