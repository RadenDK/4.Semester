using System.Threading.RateLimiting;
using AspNetCoreRateLimit;
using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.BusinessLogic.Interfaces;
using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.DatabaseAccess.Interfaces;
using FoosballProLeague.Api.Hubs;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSignalR();

// All is added as singleton instead of scoped because matchlogic caches data in forms of pending match teams
// Because of this, we want to make sure that the same instance is used for all requests
// If it were scoped, it would be created for each request, and the cache would be empty for each request
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

// Add Rate limiting services
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("RatePolicy", confic =>
    
    {
        confic.Window = TimeSpan.FromMinutes(1);
        confic.PermitLimit = 100;
        confic.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        confic.QueueLimit = 1;
    });
});

//cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.AllowAnyMethod()
               .AllowAnyHeader()
               .SetIsOriginAllowed(origin => true) // Allows all origins
               .AllowCredentials();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Use rate limiting middleware
app.UseRateLimiter();
app.MapControllers().RequireRateLimiting("RatePolicy");

app.MapControllers();

// Map the SignalR hub
app.MapHub<HomepageHub>("/homepageHub");


app.Run();