using FoosballProLeague.Webserver.Models;

namespace FoosballProLeague.Webserver.BusinessLogic
{
    public interface IAccountLogic
    {
        public Task<HttpResponseMessage> FindUserByEmail(string email);
        public Task SendEmail(string email);
        public Task<HttpResponseMessage> ResetPassword(string email, string newPassword);
    }
}
