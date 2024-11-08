using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.DatabaseAccess;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<ICompanyLogic, CompanyLogic>();
builder.Services.AddScoped<ICompanyDatabaseAccessor, CompanyDatabaseAccessor>();
builder.Services.AddScoped<IUserLogic, UserLogic>();
builder.Services.AddScoped<IUserDatabaseAccessor, UserDatabaseAccessor>();
builder.Services.AddScoped<IDepartmentLogic, DepartmentLogic>();
builder.Services.AddScoped<IDepartmentDatabaseAccessor, DepartmentDatabaseAccessor>();
builder.Services.AddScoped<IMatchLogic, MatchLogic>();
builder.Services.AddScoped<IMatchDatabaseAccessor, MatchDatabaseAccessor>();

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();