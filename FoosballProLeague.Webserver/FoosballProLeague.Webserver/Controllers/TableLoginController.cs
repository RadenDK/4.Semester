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
using System.Diagnostics;

public class TableLoginController : Controller
{
    private readonly ITableLoginLogic _tableLoginLogic;

    public TableLoginController(ITableLoginLogic tableLoginLogic)
    {
        _tableLoginLogic = tableLoginLogic;
    }

    

    [HttpPost("TableLoginRed")]
    public async Task<IActionResult> TableLogin([FromBody] TableLoginModel tableLoginModel)
    {
        if (tableLoginModel == null)
        {
            return BadRequest("Invalid login model.");
        }

        if (tableLoginModel.Side == "blue")
        {
            return View("TableLoginBlueSide", tableLoginModel);
        }

        return View("TableLoginRedSide", tableLoginModel);
    }

    [HttpGet("ClearTeam")]
    public async Task<IActionResult> ClearTeam(string side)
    {
        await _tableLoginLogic.TableClearTeam();

        if (side == "blue")
        {
            return View("TableLoginBlueSide");
        }

        return View("TableLoginRedSide");
    }
}