using FoosballProLeague.Webserver.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace FoosballProLeague.Webserver.Controllers
{
    public class JwtAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            string jwt = context.HttpContext.Request.Cookies["accessToken"];
            ITokenLogic tokenLogic = context.HttpContext.RequestServices.GetService<ITokenLogic>();

            if (string.IsNullOrEmpty(jwt) || tokenLogic == null)
            {
                RedirectToLogin(context);
                return;
            }

            string newJwt = await ValidateAndGetNewJwt(tokenLogic, jwt);

            if (!string.IsNullOrEmpty(newJwt))
            {
                context.HttpContext.Response.Cookies.Append("accessToken", newJwt);
            }
            else
            {
                RedirectToLogin(context);
            }
        }

        private void RedirectToLogin(AuthorizationFilterContext context)
        {
            context.Result = new RedirectToActionResult("Login", "Login", new { ReturnUrl = context.HttpContext.Request.Path });
        }

        private async Task<string> ValidateAndGetNewJwt(ITokenLogic tokenLogic, string jwt)
        {
            return await tokenLogic.ValidateAndGetNewJwt(jwt);
        }
    }
}
