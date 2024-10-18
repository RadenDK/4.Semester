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
        
        public UserController(IUserLogic userLogic)
        {
            _userLogic = userLogic;
        }
        
        // method to handle registration of a new user (create user)
        [HttpPost("user")]
        public IActionResult CreateUser(UserRegistrationModel userRegistrationModel)
        {
            if(_userLogic.CreateUser(userRegistrationModel))
            {
                return Ok(); // Return Ok if the user was created successfully
            }
            else
            {
                return BadRequest(new { message = "Error creating user" });
            }
        }
        
        // method to get all users in a list
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
        
        
        // Method to handle user login
        [HttpPut("login")]
        
        public IActionResult LoginUser(UserLoginModel userLoginModel)
        {
            try
            {
                if(_userLogic.LoginUser(userLoginModel.Email, userLoginModel.Password))
                {
                    return Ok(); // Return Ok if the user was logged in successfully
                }
                else
                {
                    return BadRequest(); // Return BadRequest if the user was not logged in successfully
                }
            } catch (Exception e)
            {
                return StatusCode(500, new { message = "An error occurred while logging in the user" });
            }
        }
    }
    
    
}