using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.BusinessLogic.Interfaces;
using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.DatabaseAccess.Interfaces;
using FoosballProLeague.Api.Models;
using Microsoft.AspNetCore.Mvc;


namespace FoosballProLeague.Api.Tests.ControllerTests.IntergrationTests.DepartmentControllerTests
{
    [Collection("Non-Parallel Database Collection")]
    public class GetDepartmentTests : DatabaseTestBase
    {
        [Fact]
        public void GetDepartments_WithDepartments_ShouldReturnSuccess()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO companies (id) VALUES (1), (2)");
            _dbHelper.InsertData("INSERT INTO departments (id, name, company_id) VALUES (1, 'Department 1', 1), (2, 'Department 2', 2)");

            IDepartmentDatabaseAccessor departmentDatabaseAccessor = new DepartmentDatabaseAccessor(_dbHelper.GetConfiguration());
            IDepartmentLogic departmentLogic = new DepartmentLogic(departmentDatabaseAccessor);
            DepartmentController SUT = new DepartmentController(departmentLogic);

            // Act
            IActionResult result = SUT.GetDepartments();

            // Assert
            IEnumerable<DepartmentModel> databaseDepartments = _dbHelper.ReadData<DepartmentModel>("SELECT * FROM departments");
            IEnumerable<DepartmentModel> resultDepartments = (result as OkObjectResult)?.Value as IEnumerable<DepartmentModel>;

            Assert.IsType<OkObjectResult>(result);

            for (int i = 0; i < databaseDepartments.Count(); i++)
            {
                DepartmentModel resultDepartment = resultDepartments.ElementAt(i);
                DepartmentModel databaseDepartment = databaseDepartments.ElementAt(i);
                Assert.True(resultDepartment.Id == databaseDepartment.Id);
                Assert.True(resultDepartment.Name == databaseDepartment.Name);
                Assert.True(resultDepartment.CompanyId == databaseDepartment.CompanyId);
            }
        }

        [Fact]
        public void GetDepartments_NoDepartments_ShouldReturnEmptyList()
        {
            // Arrange
            IDepartmentDatabaseAccessor departmentDatabaseAccessor = new DepartmentDatabaseAccessor(_dbHelper.GetConfiguration());
            IDepartmentLogic departmentLogic = new DepartmentLogic(departmentDatabaseAccessor);
            DepartmentController SUT = new DepartmentController(departmentLogic);

            // Act
            IActionResult result = SUT.GetDepartments();

            // Assert
            IEnumerable<DepartmentModel> resultDepartments = (result as OkObjectResult)?.Value as IEnumerable<DepartmentModel>;

            Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(resultDepartments);
            Assert.Empty(resultDepartments);
        }
    }
}
