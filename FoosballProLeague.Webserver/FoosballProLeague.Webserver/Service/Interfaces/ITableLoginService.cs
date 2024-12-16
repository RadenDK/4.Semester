using FoosballProLeague.Webserver.Models;
using System.Net.Http;
using System.Threading.Tasks;


namespace FoosballProLeague.Webserver.Service.Interfaces;

public interface ITableLoginService
{
    public Task<HttpResponseMessage> TableLoginUser(TableLoginViewModel tableLoginModel);
    public Task<HttpResponseMessage> PendingUsers(int tableId);
    public  Task<HttpResponseMessage> RemoveUser(string email);

}