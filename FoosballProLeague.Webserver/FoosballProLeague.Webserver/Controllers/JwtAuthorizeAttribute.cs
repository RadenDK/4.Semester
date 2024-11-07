using FoosballProLeague.Webserver.BusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FoosballProLeague.Webserver.Controllers
{
    public class JwtAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            string jwt = context.HttpContext.Request.Cookies["accessToken"];
            if (string.IsNullOrEmpty(jwt))
            {
                RedirectToLogin(context);
                return;
            }

            ITokenLogic tokenLogic = context.HttpContext.RequestServices.GetService<ITokenLogic>();
            if (tokenLogic == null || !ValidateToken(tokenLogic, jwt))
            {
                RedirectToLogin(context);
            }
        }

        private void RedirectToLogin(AuthorizationFilterContext context)
        {
            context.Result = new RedirectToActionResult("Login", "Login", new { ReturnUrl = context.HttpContext.Request.Path });
        }

        private bool ValidateToken(ITokenLogic tokenLogic, string jwt)
        {
            return tokenLogic.ValidateJwt(jwt).GetAwaiter().GetResult();
        }
    }
}
