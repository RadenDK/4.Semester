namespace FoosballProLeague.Webserver.Service
{
    public interface IAccountService
    {
        public Task<HttpResponseMessage> FindUserByEmail(string email);
        public Task<HttpResponseMessage> ResetPassword(string email, string password);
        public Task<HttpResponseMessage> SendEmail(string email);
    }
}
