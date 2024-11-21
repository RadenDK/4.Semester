using System.Threading.RateLimiting;
using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.BusinessLogic.Interfaces;
using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.DatabaseAccess.Interfaces;
using FoosballProLeague.Api.Hubs;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Register all services as singletons to maintain shared state
builder.Services.AddSingleton<ICompanyLogic, CompanyLogic>();
builder.Services.AddSingleton<ICompanyDatabaseAccessor, CompanyDatabaseAccessor>();
builder.Services.AddSingleton<IUserLogic, UserLogic>();
builder.Services.AddSingleton<IUserDatabaseAccessor, UserDatabaseAccessor>();
builder.Services.AddSingleton<IDepartmentLogic, DepartmentLogic>();
builder.Services.AddSingleton<IDepartmentDatabaseAccessor, DepartmentDatabaseAccessor>();
builder.Services.AddSingleton<IMatchLogic, MatchLogic>();
builder.Services.AddSingleton<IMatchDatabaseAccessor, MatchDatabaseAccessor>();
builder.Services.AddSingleton<ITeamDatabaseAccessor, TeamDatabaseAccessor>();
builder.Services.AddSingleton<ITokenLogic, TokenLogic>();

// Add SignalR support
builder.Services.AddSignalR();

// Add CORS with fixed configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.SetIsOriginAllowed(_ => true) // More secure than AllowAnyOrigin
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

// Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure Kestrel to listen on all IPs (required for Docker)
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5001);  // Listen on all interfaces, necessary for Docker
});

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// IMPORTANT: UseCors must be called before UseRouting and MapHub
app.UseCors("CorsPolicy");

app.UseRouting(); // Add this line

app.UseAuthorization();

// Map SignalR hub
app.MapHub<HomepageHub>("/homepageHub"); // Make sure to specify the Hub class

// Map controllers
app.MapControllers();

app.Run();
