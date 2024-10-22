using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoosballProLeague.Api.Tests
{
    /// <summary>
    /// DatabaseTestBase is an abstract class that provides a common setup and teardown mechanism for all tests that 
    /// involve database interactions in the FoosballProLeague API project.
    /// 
    /// Inheriting from DatabaseTestBase allows each test to automatically have its database cleared before every 
    /// test run, ensuring a consistent and clean state for each individual test, preventing cross-test data pollution.
    /// 
    /// This class implements the IAsyncLifetime interface from xUnit, which provides two key methods:
    /// 
    /// 1. **InitializeAsync**: 
    ///     - This method is executed before each test.
    ///     - It ensures that the database is cleared before each test by calling the `ClearDatabase` method from DatabaseHelper.
    ///     - This guarantees that tests are isolated and do not interfere with each other.
    /// 
    /// 2. **DisposeAsync**: 
    ///     - This method runs after each test. It provides an opportunity for optional cleanup operations after each test.
    ///     - In this case, no specific teardown logic is needed, but the method is implemented to allow flexibility for future extensions.
    /// 
    /// By inheriting from DatabaseTestBase, all test classes in the project will benefit from automatic database cleanup, 
    /// streamlining test setup and ensuring reliability and consistency in test results.
    /// </summary>

    public abstract class DatabaseTestBase : IAsyncLifetime
    {
        protected readonly DatabaseHelper _dbHelper;

        public DatabaseTestBase()
        {
            _dbHelper = new DatabaseHelper(); // Create an instance of DatabaseHelper
        }

        // This method will run before each test
        public Task InitializeAsync()
        {
            _dbHelper.ClearDatabase(); // Automatically clear the database before each test
            return Task.CompletedTask;
        }

        // This method will run after each test (optional cleanup)
        public Task DisposeAsync()
        {
            // You can add additional cleanup logic here if needed
            return Task.CompletedTask;
        }
    }
}
