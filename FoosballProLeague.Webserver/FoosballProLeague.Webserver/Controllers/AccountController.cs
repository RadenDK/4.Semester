using FoosballProLeague.Webserver.BusinessLogic;
using FoosballProLeague.Webserver.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace FoosballProLeague.Webserver.Controllers
{
    public class AccountController : Controller
    {

        private readonly IAccountLogic _accountLogic;

        public AccountController(IAccountLogic accountLogic)
        {
            _accountLogic = accountLogic;
        }


        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel forgotPasswordModel)
        {
            if (ModelState.IsValid)
            {
                var response = await _accountLogic.FindUserByEmail(forgotPasswordModel.Email);
                if (response != null)
                {
                    await _accountLogic.SendEmail(forgotPasswordModel.Email);
                }

                return RedirectToAction("ForgotPasswordConfirmation");
            }
            return View(forgotPasswordModel);
        }

        [HttpPut]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordModel)
        {
            if (ModelState.IsValid)
            {
                HttpResponseMessage result = await _accountLogic.ResetPassword(resetPasswordModel.Email, resetPasswordModel.Password);
                if (result.IsSuccessStatusCode)
                {
                    return RedirectToAction("ResetPasswordConfirmation");
                }

                ModelState.AddModelError(string.Empty, "Error resetting password.");
            }

            return View(resetPasswordModel);
        }
    }
}
