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
        
        return await _tableLoginService.TableLoginUser(tableLoginModel);
    }


    public async Task<HttpResponseMessage> TableClearTeam()
    {
        return await _tableLoginService.TableClearTeam();
    }
}