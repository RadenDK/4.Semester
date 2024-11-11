using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.Models;

namespace FoosballProLeague.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private IUserLogic _userLogic;
        
        public UserController(IUserLogic userLogic)
        {
            _userLogic = userLogic;
        }
        
        // method to handle registration of a new user (create user)
        [HttpPost]
        public IActionResult CreateUser(UserRegistrationModel userRegistrationModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            try
            {
                if (_userLogic.GetUser(userRegistrationModel.Email) != null)
                {
                    return BadRequest(new { message = "Email already exists" });
                }
                
                if(_userLogic.CreateUser(userRegistrationModel))
                {
                    return Ok(); // Return Ok if the user was created successfully
                }
                else
                {
                    return BadRequest(new { message = "Error creating user" });
                }
            }
            catch (ArgumentException e)
            {
                return BadRequest(new { message = e.Message });
            }
        }
        
        // method to get all users in a list
        [HttpGet]
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
        [HttpPost("login")]
        
        public IActionResult LoginUser(UserLoginModel userLoginModel)
        {
            try
            {
                bool loginSucces = _userLogic.LoginUser(userLoginModel.Email, userLoginModel.Password);
                if(loginSucces)
                {
                    return Ok(); // Return Ok if the user was logged in successfully
                }
                else
                {
                    return BadRequest(new { message = "Email or password is incorrect"}); // Return BadRequest if the user was not logged in successfully
                }
            } catch (Exception e)
            {
                return StatusCode(500, new { message = "An error occurred while logging in the user" });
            }
        }

        [HttpGet("user/userId/match-history")]
        public IActionResult GetMatchHistoryByUserId(int userId)
        {
            try
            {
                IEnumerable<MatchHistoryModel> matchHistory = _userLogic.GetMatchHistoryByUserId(userId);
                if (matchHistory == null || !matchHistory.Any())
                {
                    return NotFound("No match history found for the provided user id");
                }

                return Ok(matchHistory);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
    
    
}