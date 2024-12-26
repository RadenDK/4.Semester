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
        List<TableLoginUserModel> pendingUsers = await _tableLoginLogic.PendingUsers(tableId);

        TableLoginViewModel tableLoginViewModel = new TableLoginViewModel
        {
            TableId = tableId,
            Side = side,
            PendingUsers = pendingUsers
        };

        return View("TableLogin", tableLoginViewModel);
    }

    [HttpPost("TableLogin/{tableId:int}/{side}")]
    public async Task<IActionResult> TableLogin(TableLoginViewModel tableLoginViewModel, int tableId, string side)
    {
        if (tableLoginViewModel == null || tableLoginViewModel.Side != side)
        {
            return BadRequest("Invalid login model or side mismatch.");
        }

        List<TableLoginUserModel> pendingUsers = await _tableLoginLogic.PendingUsers(tableId);
        HttpResponseMessage response = await _tableLoginLogic.ActiveMatch();

        string responseContent = await response.Content.ReadAsStringAsync();
        bool isActiveMatch = !string.IsNullOrEmpty(responseContent);

        if (!isActiveMatch)
        {
            int userCountOnSide = pendingUsers.Count(u => u.Side == side);

            if (userCountOnSide >= 2)
            {
                ModelState.AddModelError(string.Empty, "Cannot add more than two users per side.");
                tableLoginViewModel.PendingUsers = pendingUsers;
                return View("TableLogin", tableLoginViewModel);
            }

            await _tableLoginLogic.TableLoginUser(tableLoginViewModel);
            pendingUsers = await _tableLoginLogic.PendingUsers(tableId);

            tableLoginViewModel.TableId = tableId;
            tableLoginViewModel.Side = side;
            tableLoginViewModel.PendingUsers = pendingUsers;

            return View("TableLogin", tableLoginViewModel);
        }

        ModelState.AddModelError(string.Empty, "A match is currently ongoing, wait for match to end");
        return View("TableLogin", tableLoginViewModel);
    }

    [HttpPost("RemovePendingUser")]
    public async Task<IActionResult> RemovePendingUser(TableLoginViewModel tableLoginViewModel)
    {
        if (string.IsNullOrEmpty(tableLoginViewModel.Email))
        {
            return BadRequest("No user selected for removal.");
        }

        await _tableLoginLogic.RemoveUser(tableLoginViewModel.Email);

        List<TableLoginUserModel> pendingUsers = await _tableLoginLogic.PendingUsers(tableLoginViewModel.TableId);

        tableLoginViewModel.PendingUsers = pendingUsers;

        return View("TableLogin", tableLoginViewModel);
    }
}