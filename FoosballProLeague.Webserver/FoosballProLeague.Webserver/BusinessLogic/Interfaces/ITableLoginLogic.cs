using FoosballProLeague.Webserver.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace FoosballProLeague.Webserver.BusinessLogic.Interfaces;

public interface ITableLoginLogic
{
    Task<HttpResponseMessage> TableLoginUser(TableLoginModel tableLoginModel);
    Task<HttpResponseMessage> TableClearTeam(int tableID);
}