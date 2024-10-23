using FoosballProLeague.Api.BusinessLogic;
using Xunit;
using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoosballProLeague.Api.Tests.ControllerTests.IntegrationTests
{
    [Collection("Non-Parallel Database Collection")]
    
    public class CreateUserTest : DatabaseTestBase
    {
        private readonly UserController _userController;
        private readonly IUserLogic _userLogic;

        public CreateUserTest()
        {
            _userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()));
            _userController = new UserController(_userLogic);
        }

        [Fact]
        public void CreateUser_ShouldReturnOk()
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
            var result = _userController.CreateUser(newUser) as OkResult;
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            
            _dbHelper.ClearDatabase();
        }

        [Fact]
        public void CreateUser_ShouldReturnBadRequest_WhenModelStateIsInvalid()
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
            var result = _userController.CreateUser(newUser) as BadRequestObjectResult;
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            
            _dbHelper.ClearDatabase();
        }
    }   
}