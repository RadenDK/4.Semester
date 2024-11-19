using FoosballProLeague.Webserver.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace FoosballProLeague.Webserver.BusinessLogic;

public interface ILoginLogic
{
    Task<HttpResponseMessage> LoginUser(string email, string password);
}