﻿using FoosballProLeague.Webserver.Models;

namespace FoosballProLeague.Webserver.BusinessLogic;

public interface ILoginLogic
{
    Task<HttpResponseMessage> LoginUser(LoginUserModel loginModel);
}