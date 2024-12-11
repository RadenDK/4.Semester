using FoosballProLeague.Webserver.BusinessLogic.Interfaces;
using FoosballProLeague.Webserver.Models;
using FoosballProLeague.Webserver.Service.Interfaces;
using System.Net.Http;

namespace FoosballProLeague.Webserver.BusinessLogic;

public class TableLoginLogic : ITableLoginLogic
{ 
    private readonly ITableLoginService _tableLoginService;

    public TableLoginLogic(ITableLoginService tableLoginService)
    {
        _tableLoginService = tableLoginService;
    }
    public async Task<HttpResponseMessage> TableLoginUser(TableLoginModel tableLoginModel)
    {
         // Deserialize the JSON content to TableLoginModel
        var tableLoginModel = JsonSerializer.Deserialize<TableLoginModel>(jsonContent);

        // Use the deserialized TableLoginModel
        return await _tableLoginService.TableLoginUser(tableLoginModel.Email, tableLoginModel.TableID, tableLoginModel.Side);
    }


    public async Task<HttpResponseMessage> TableClearTeam(int tableID)
    {
        return await _tableLoginService.TableClearTeam(tableID);
    }
}