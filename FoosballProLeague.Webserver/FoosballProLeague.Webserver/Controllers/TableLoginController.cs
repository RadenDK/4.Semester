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

    [HttpGet("TableLogin/{tableId:int}/{side}")]
    public async Task<IActionResult> TableLoginIndex(int tableId, string side)
    {
        TableLoginModel tableLoginModel = new TableLoginModel
        {
            TableId = tableId,
            Side = side
        };

        return View("TableLogin", tableLoginModel);
    }

    [HttpPost("TableLogin/{tableId:int}/{side}")]
    public async Task<IActionResult> TableLogin(TableLoginModel tableLoginModel, int tableId, string side)
    {
        if (tableLoginModel == null || tableLoginModel.Side != side)
        {
            return BadRequest("Invalid login model or side mismatch.");
        }

        tableLoginModel.TableId = tableId;
        tableLoginModel.Side = side;

        await _tableLoginLogic.TableLoginUser(tableLoginModel);

        return View("TableLogin", tableLoginModel);
    }

    [HttpGet("RemoveUser")]
    public async Task<IActionResult> RemoveUser(TableLoginModel tableLoginModel)
    {
        await _tableLoginLogic.RemoveUser(tableLoginModel);

        return View("TableLogin", new TableLoginModel());
    }
}