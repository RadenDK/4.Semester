using FoosballProLeague.Webserver.Models;
using System.Net.Http;
using System.Threading.Tasks;


namespace FoosballProLeague.Webserver.Service.Interfaces;

public interface ILoginService
{
    Task<HttpResponseMessage> LoginUser(string email, string password);
}