﻿using FoosballProLeague.Webserver.Models;
using System.Net.Http;
using System.Threading.Tasks;


namespace FoosballProLeague.Webserver.Service;

public interface ILoginService
{
    public Task<HttpResponseMessage> LoginUser(string email, string password);
}