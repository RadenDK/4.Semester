using FoosballProLeague.Webserver.Models;


namespace FoosballProLeague.Webserver.Service.Interfaces;

public interface ITableLoginService
{
    public Task<Dictionary<string, List<UserModel>>> GetAllCurrentPendingUsers(int tableId);
    public Task<HttpResponseMessage> TableLoginUser(TableLoginModel tableLoginModel);
    public  Task<HttpResponseMessage> RemoveUser(int userId, int tableId);

}