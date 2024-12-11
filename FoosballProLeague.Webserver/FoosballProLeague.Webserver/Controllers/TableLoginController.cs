using System.Text.Json;
using FoosballProLeague.Webserver.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using FoosballProLeague.Webserver.BusinessLogic.Interfaces;

namespace FoosballProLeague.Webserver.Controllers;

using Microsoft.AspNetCore.Mvc;

public class TableLoginController : Controller
{
    private readonly ITableLoginLogic _tableLoginLogic;

    public TableLoginController(ITableLoginLogic tableLoginLogic)
    {
        _tableLoginLogic = tableLoginLogic;
    }   

 [HttpPost]
    public async Task<IActionResult> TableLoginBlueSide([FromBody] TableLoginModel tableLoginModel)
    {
        
        if (tableLoginModel.side == "blue"){
            return View("TableLoginBlueSide", tableLoginModel)  ;
        }
        else if (tableLoginModel.side == "red"){
            return View("TableLoginRedSide", tableLoginModel);
        }
        
    }
[HttpGet]
public async Task<IActionResult> TableClearTeam()
{
    
}

}