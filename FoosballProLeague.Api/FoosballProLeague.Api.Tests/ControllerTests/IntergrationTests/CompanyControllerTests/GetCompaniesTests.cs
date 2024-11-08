using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoosballProLeague.Api.Tests.ControllerTests.IntergrationTests.CompanyControllerTests
{

    [Collection("Non-Parallel Database Collection")]
    public class GetCompaniesTests : DatabaseTestBase
    {
        [Fact]
        public void GetCompanies_WithCompanies_ShouldReturnSuccess()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO companies (id, name) VALUES (1, 'Company 1'), (2, 'Company 2')");

            ICompanyDatabaseAccessor companyDatabaseAccessor = new CompanyDatabaseAccessor(_dbHelper.GetConfiguration());
            ICompanyLogic companyLogic = new CompanyLogic(companyDatabaseAccessor);
            CompanyController SUT = new CompanyController(companyLogic);

            // Act
            IActionResult result = SUT.GetCompanies();

            // Assert
            IEnumerable<CompanyModel> databaseCompanies = _dbHelper.ReadData<CompanyModel>("SELECT * FROM companies");
            IEnumerable<CompanyModel> resultCompanies = (result as OkObjectResult)?.Value as IEnumerable<CompanyModel>;

            Assert.IsType<OkObjectResult>(result);
            
            for (int i = 0; i < databaseCompanies.Count(); i++)
            {
                CompanyModel resultCompany = resultCompanies.ElementAt(i);
                CompanyModel databaseCompany = databaseCompanies.ElementAt(i);
                Assert.True(resultCompany.Id == databaseCompany.Id);
                Assert.True(resultCompany.Name == databaseCompany.Name);
            }
        }

        [Fact]
        public void GetCompanies_NoCompanies_ShouldReturnEmptyList()
        {
            // Arrange
            ICompanyDatabaseAccessor companyDatabaseAccessor = new CompanyDatabaseAccessor(_dbHelper.GetConfiguration());
            ICompanyLogic companyLogic = new CompanyLogic(companyDatabaseAccessor);
            CompanyController SUT = new CompanyController(companyLogic);

            // Act
            IActionResult result = SUT.GetCompanies();

            // Assert
            IEnumerable<CompanyModel> resultCompanies = (result as OkObjectResult)?.Value as IEnumerable<CompanyModel>;

            Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(resultCompanies);
            Assert.Empty(resultCompanies); 
        }
    }
}
