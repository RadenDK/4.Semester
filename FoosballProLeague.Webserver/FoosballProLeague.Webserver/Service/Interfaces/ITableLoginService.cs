using FoosballProLeague.Webserver.Models;
using System.Net.Http;
using System.Threading.Tasks;


namespace FoosballProLeague.Webserver.Service.Interfaces;

public interface ITableLoginService
{
    public Task<HttpResponseMessage> TableLoginUser(TableLoginModel tableLoginModel);
    public  Task<HttpResponseMessage> RemoveUser(TableLoginModel tableLoginModel);

}