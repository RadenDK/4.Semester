using FoosballProLeague.Webserver.BusinessLogic.Interfaces;
using FoosballProLeague.Webserver.Models;
using FoosballProLeague.Webserver.Service.Interfaces;
using Newtonsoft.Json;
using System.Net.Http;

namespace FoosballProLeague.Webserver.BusinessLogic;

public class TableLoginLogic : ITableLoginLogic
{ 
    private readonly ITableLoginService _tableLoginService;

    public TableLoginLogic(ITableLoginService tableLoginService)
    {
        _tableLoginService = tableLoginService;
    }

    public async Task<HttpResponseMessage> TableLoginUser(TableLoginViewModel tableLoginModel)
    {
        return await _tableLoginService.TableLoginUser(tableLoginModel);
    }

    public async Task<List<TableLoginUserModel>> PendingUsers(int tableId)
    {
        HttpResponseMessage response = await _tableLoginService.PendingUsers(tableId);
        string responseBody = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<List<TableLoginUserModel>>(responseBody);
    }


    public async Task<HttpResponseMessage> RemoveUser(string email)
    {
        return await _tableLoginService.RemoveUser(email);
    }

    public async Task<HttpResponseMessage> ActiveMatch()
    {
        return await _tableLoginService.ActiveMatch();
    }
}