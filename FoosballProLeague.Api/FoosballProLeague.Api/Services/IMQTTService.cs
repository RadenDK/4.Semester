namespace FoosballProLeague.Api.Services;

public interface IMQTTService
{
    Task ConnectAsync();
    Task PublishMessageAsync(string message);
}