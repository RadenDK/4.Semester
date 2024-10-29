using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.Models;
using Microsoft.AspNetCore.SignalR;
using FoosballProLeague.Api.Hubs; 


namespace FoosballProLeague.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private IUserLogic _userLogic;
        private readonly IHubContext<LeaderboardHub> _hubContext; 
        private readonly LeaderboardService _leaderboardService;

        public UserController(IUserLogic userLogic, IHubContext<LeaderboardHub> hubContext, LeaderboardService leaderboardService)
        {
            _userLogic = userLogic;
            _hubContext = hubContext;
            _leaderboardService = leaderboardService;
        }
        
        // method to handle registration of a new user (create user)
        [HttpPost]
        public async Task<IActionResult> CreateUser(UserRegistrationModel userRegistrationModel)
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
                    await _leaderboardService.NotifyLeaderboardChange();
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
        
        [HttpGet("test-update")]
        public async Task<IActionResult> TestUpdate()
        {
            await _leaderboardService.NotifyLeaderboardChange();
            return Ok("Update sent");
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
    }
    
    
}