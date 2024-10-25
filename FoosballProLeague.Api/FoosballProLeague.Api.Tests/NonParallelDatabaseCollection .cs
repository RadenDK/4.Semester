using Xunit;

namespace FoosballProLeague.Api.Tests
{
    // This collection definition ensures that all tests using this collection
    // will run sequentially and share the same DatabaseHelper instance.
    //
    // 1. ICollectionFixture<DatabaseHelper> is used to share a single instance
    //    of DatabaseHelper across all test classes in this collection, preventing 
    //    the need to instantiate it for each test class.
    //
    // 2. DisableParallelization = true ensures that the tests within this collection 
    //    run sequentially (one at a time) rather than in parallel, which is important 
    //    to avoid concurrency issues, especially when dealing with shared resources 
    //    like databases.

    [CollectionDefinition("Non-Parallel Database Collection", DisableParallelization = true)]
    public class NonParallelDatabaseCollection : ICollectionFixture<DatabaseHelper>
    {
        // No additional code is needed here, as this class simply applies the collection definition
    }
}
