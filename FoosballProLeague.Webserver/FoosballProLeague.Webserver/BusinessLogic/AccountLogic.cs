using FoosballProLeague.Webserver.Models;
using FoosballProLeague.Webserver.Service;

namespace FoosballProLeague.Webserver.BusinessLogic
{
    public class AccountLogic : IAccountLogic
    {

        private readonly IAccountService _accountService;


        public AccountLogic(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<HttpResponseMessage> FindUserByEmail(string email)
        {
            return await _accountService.FindUserByEmail(email);
        }

        public async Task SendEmail(string email)
        {
            await _accountService.SendEmail(email);
        }

        public async Task<HttpResponseMessage> ResetPassword(string email, string newPassword)
        {
            return await _accountService.ResetPassword(email, newPassword);
        }
    }
}
