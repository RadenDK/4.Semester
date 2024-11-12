using System.Threading.RateLimiting;
using AspNetCoreRateLimit;
using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.DatabaseAccess;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<ICompanyLogic, CompanyLogic>();
builder.Services.AddScoped<ICompanyDatabaseAccessor, CompanyDatabaseAccessor>();

builder.Services.AddScoped<IUserLogic, UserLogic>();
builder.Services.AddScoped<IUserDatabaseAccessor, UserDatabaseAccessor>();

builder.Services.AddScoped<IDepartmentLogic, DepartmentLogic>();
builder.Services.AddScoped<IDepartmentDatabaseAccessor, DepartmentDatabaseAccessor>();

builder.Services.AddScoped<ITokenLogic, TokenLogic>();

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

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Use rate limiting middleware
app.UseRateLimiter();
app.MapControllers().RequireRateLimiting("RatePolicy");

app.MapControllers();

app.Run();