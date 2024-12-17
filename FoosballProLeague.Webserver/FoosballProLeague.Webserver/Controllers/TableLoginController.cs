using FoosballProLeague.Webserver.BusinessLogic.Interfaces;
using FoosballProLeague.Webserver.Models;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;

[Route("TableLogin")]
public class TableLoginController : Controller
{
    private readonly ITableLoginLogic _tableLoginLogic;

    public TableLoginController(ITableLoginLogic tableLoginLogic)
    {
        _tableLoginLogic = tableLoginLogic;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int tableId, string side)
    {
        ViewBag.TableId = tableId;
        Dictionary<string, List<UserModel>> pendingUsers = await _tableLoginLogic.GetAllCurrentPendingUsers(tableId);

        TableStatusViewModel model = new TableStatusViewModel
        {
            Side = side,
            PendingUsers = pendingUsers
        };

        return View("TableLogin", model);
    }

    [HttpPost]
    public async Task<IActionResult> TableLogin(int tableId, string side, string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return BadRequest("Email cannot be null or empty.");
        }

        TableLoginModel loginModel = new TableLoginModel
        {
            TableId = tableId,
            Side = side,
            Email = email
        };

        await _tableLoginLogic.TableLoginUser(loginModel);

        return Ok();
    }

    [HttpPost("RemoveUser")]
    public async Task<IActionResult> RemoveUser(int userId, int tableId)
    {
        Console.WriteLine($"Removing user {userId} from table {tableId}");


        await _tableLoginLogic.RemoveUser(userId, tableId);

        // Redirect back to the same view (Index) to reload the pending users
        return Ok();
    }

}
