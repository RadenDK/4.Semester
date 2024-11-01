using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.Models;

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
                    // Since the login was successful, generate a JWT for the user
                    string jwt = _tokenLogic.GenerateJWT();
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

        // Method to validate user JWT
        [HttpGet("token/validate")]
        public IActionResult ValidateUserJWT()
        {
            try
            {
                if (!Request.Headers.ContainsKey("Authorization"))
                {
                    return Unauthorized("No JWT token found in the request");
                }

                string jwt = Request.Headers["Authorization"];

                if (_tokenLogic.ValidateJWT(jwt))
                {
                    return Ok("JWT is valid");
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
    }
    
    
}