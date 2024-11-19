using FoosballProLeague.Api.BusinessLogic.Interfaces;
using FoosballProLeague.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoosballProLeague.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiKeyAuthorize]
    public class UserController : Controller
    {
        private IUserLogic _userLogic;
        private ITokenLogic _tokenLogic;

        public UserController(IUserLogic userLogic, ITokenLogic tokenLogic)
        {
            _userLogic = userLogic;
            _tokenLogic = tokenLogic;
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
                if (_userLogic.GetUserByEmail(userRegistrationModel.Email) != null)
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
                List<UserModel> users = _userLogic.GetAllUsers();
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
                    // Since the login was successful, generate a JWT for the user

                    UserModel user = _userLogic.GetUserByEmail(userLoginModel.Email);
                    string jwt = _tokenLogic.GenerateJWT(user);
                    return Ok(jwt); // Return Ok if the user was logged in successfully and the JWT was generated
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

        // Method to validate user JWT and generate a new JWT on every request
        [HttpGet("token/validate")]
        public IActionResult ValidateUserJWT()
        {
            try
            {
                if (!Request.Headers.ContainsKey("Authorization"))
                {
                    return Unauthorized("No JWT token found in the request");
                }

                string authorizationHeader = Request.Headers["Authorization"];

                if (_tokenLogic.ValidateJWT(authorizationHeader))
                {
                    int userIdInToken = _tokenLogic.GetUserIdFromJWT(authorizationHeader);
                    
                    UserModel userFromToken = _userLogic.GetUserById(userIdInToken);

                    string newJwt = _tokenLogic.GenerateJWT(userFromToken);

                    return Ok(newJwt);
                }
                else
                {
                    return Unauthorized("JWT token is not valid");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = "An error occurred while validating the user" });
            }
        }

        [HttpGet("{userId}/match-history")]
        public IActionResult GetMatchHistoryByUserId(int userId)
        {
            try
            {
                List<MatchHistoryModel> matchHistory = _userLogic.GetMatchHistoryByUserId(userId);
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