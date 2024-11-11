using FoosballProLeague.Api.BusinessLogic;
using Xunit;
using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.Hubs;
using FoosballProLeague.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.AspNetCore.SignalR;

namespace FoosballProLeague.Api.Tests.ControllerTests.IntegrationTests
{
    [Collection("Non-Parallel Database Collection")]
    
    public class CreateUserTest : DatabaseTestBase
    {
        private readonly UserController _userController;
        private readonly IUserLogic _userLogic;

        public CreateUserTest()
        {
            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();
            Mock<IHubClients> mockClients = new Mock<IHubClients>();
            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();

            mockHubContext.Setup(hub => hub.Clients).Returns(mockClients.Object);
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            _userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);
            _userController = new UserController(_userLogic);
        }

        [Fact]
        public async void CreateUser_ShouldReturnOk()
        {
            // Arrange
            _dbHelper.ClearDatabase();
    
            string insertCompanyQuery = "INSERT INTO companies (id, name) VALUES (1, 'Test Company')";
            _dbHelper.InsertData(insertCompanyQuery);

            string insertDepartmentQuery = "INSERT INTO departments (name, company_id) VALUES ('Test Department', 1)";
            _dbHelper.InsertData(insertDepartmentQuery);
    
            UserRegistrationModel newUser = new UserRegistrationModel
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Password = "Password123!",
                DepartmentId = 1,
                CompanyId = 1
            };
    
            // Act
            var result = await _userController.CreateUser(newUser) as OkResult;
    
            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
    
            _dbHelper.ClearDatabase();
        }

        [Fact]
        public async void CreateUser_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _dbHelper.ClearDatabase();
    
            string insertCompanyQuery = "INSERT INTO companies (id, name) VALUES (1, 'Test Company')";
            _dbHelper.InsertData(insertCompanyQuery);

            string insertDepartmentQuery = "INSERT INTO departments (name, company_id) VALUES ('Test Department', 1)";
            _dbHelper.InsertData(insertDepartmentQuery);
    
            UserRegistrationModel newUser = new UserRegistrationModel
            {
                FirstName = "John",
                LastName = "Doe",
                Password = "Password123!",
                DepartmentId = 1,
                CompanyId = 1
            };
    
            _userController.ModelState.AddModelError("Email", "Email is required");
    
            // Act
            var result = await _userController.CreateUser(newUser) as BadRequestObjectResult;            
    
            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
    
            _dbHelper.ClearDatabase();
        }

        [Fact]
        public async void CreateUser_ShouldReturnBadRequest_WhenEmailAlreadyExists()
        {
            // Arrange
            _dbHelper.ClearDatabase();
            
            string insertCompanyQuery = "INSERT INTO companies (id, name) VALUES (1, 'Test Company')";
            _dbHelper.InsertData(insertCompanyQuery);

            string insertDepartmentQuery = "INSERT INTO departments (name, company_id) VALUES ('Test Department', 1)";
            _dbHelper.InsertData(insertDepartmentQuery);
            
            string insertUserQuery = @"INSERT INTO users (first_name, last_name, email, password, department_id, company_id, elo_1v1, elo_2v2)
                                    VALUES ('Existing', 'User', 'existing.user@example.com', 'Password123!', 1, 1, 500, 500)";
            _dbHelper.InsertData(insertUserQuery);

            UserRegistrationModel newUser = new UserRegistrationModel
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "existing.user@example.com",
                Password = "Password123!",
                DepartmentId = 1,
                CompanyId = 1
            };
            
            // Act
            var result = await _userController.CreateUser(newUser) as BadRequestObjectResult;
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            
            _dbHelper.ClearDatabase();
        }
    }   
}