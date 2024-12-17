using FoosballProLeague.Webserver.BusinessLogic.Interfaces;
using FoosballProLeague.Webserver.Models;
using FoosballProLeague.Webserver.Service.Interfaces;

namespace FoosballProLeague.Webserver.BusinessLogic
{
    public class TableLoginLogic : ITableLoginLogic
    {
        private readonly ITableLoginService _tableLoginService;

        public TableLoginLogic(ITableLoginService tableLoginService)
        {
            _tableLoginService = tableLoginService;
        }

        public async Task<Dictionary<string, List<UserModel>>> GetAllCurrentPendingUsers(int tableId)
        {
            return await _tableLoginService.GetAllCurrentPendingUsers(tableId);
        }

        public async Task<HttpResponseMessage> TableLoginUser(TableLoginModel tableLoginModel)
        {
            return await _tableLoginService.TableLoginUser(tableLoginModel);
        }


        public async Task<HttpResponseMessage> RemoveUser(int userId, int tableId)
        {
            return await _tableLoginService.RemoveUser(userId, tableId);
        }
    }
}
