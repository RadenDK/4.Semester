using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.DatabaseAccess;

var builder = WebApplication.CreateBuilder(args);

// Add configuration sources in the desired order
builder.Configuration
       .AddEnvironmentVariables()      // Prioritizes environment variables over appsettings.json
       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true); // Fallback to appsettings.json

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<ICompanyLogic, CompanyLogic>();
builder.Services.AddScoped<ICompanyDatabaseAccessor, CompanyDatabaseAccessor>();
builder.Services.AddScoped<IUserLogic, UserLogic>();
builder.Services.AddScoped<IUserDatabaseAccessor, UserDatabaseAccessor>();
builder.Services.AddScoped<IDepartmentLogic, DepartmentLogic>();
builder.Services.AddScoped<IDepartmentDatabaseAccessor, DepartmentDatabaseAccessor>();

// Configure Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.Run();
