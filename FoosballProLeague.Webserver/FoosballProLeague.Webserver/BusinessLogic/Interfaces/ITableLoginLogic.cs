using FoosballProLeague.Webserver.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace FoosballProLeague.Webserver.BusinessLogic.Interfaces;

public interface ITableLoginLogic
{
    Task<HttpResponseMessage> TableLoginUser(TableLoginViewModel tableLoginModel);
    Task<List<TableLoginUserModel>> PendingUsers(int tableId);
    Task<HttpResponseMessage> RemoveUser(string email);
    public Task<HttpResponseMessage> ActiveMatch();
}