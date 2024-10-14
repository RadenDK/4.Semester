using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.Models;

namespace FoosballProLeague.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private IUserLogic _userLogic;
        
        [HttpPost("user")]
        public IActionResult CreateUser(UserRegistrationModel userRegistrationModel)
        {
            if(_userLogic.CreateUser(userRegistrationModel))
            {
                return Ok(); // Return Ok if the user was created successfully
            }
            else
            {
                return BadRequest(new { message = "Error creating the player" });
            }
        }

        [HttpGet("users")]
        public IActionResult GetUsers()
        {
            try
            {
                List<UserModel> users = _userLogic.GetUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}