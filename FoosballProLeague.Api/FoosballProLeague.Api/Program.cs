using System.Threading.RateLimiting;
using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.BusinessLogic.Interfaces;
using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.DatabaseAccess.Interfaces;
using FoosballProLeague.Api.Hubs;

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

// Add CORS to allow everything for simplicity
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.AllowAnyMethod()
               .AllowAnyHeader()
               .AllowAnyOrigin() // Allow all origins
               .AllowCredentials();
    });
});

// Disable rate limiting for simplicity
// Note: Rate limiting is not added to ensure all requests, including SignalR, are processed without restrictions

// Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use CORS
app.UseCors("CorsPolicy");

// Remove HTTPS redirection for simplicity
// app.UseHttpsRedirection();

app.UseAuthorization();

// Map SignalR hub
app.MapHub<HomepageHub>("/homepageHub");

// Map controllers
app.MapControllers();

app.Run();
