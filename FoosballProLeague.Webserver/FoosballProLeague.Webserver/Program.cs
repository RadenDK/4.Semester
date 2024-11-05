using FoosballProLeague.Webserver.BusinessLogic;
using FoosballProLeague.Webserver.Service;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register Services
builder.Services.AddHttpClient<IHttpClientService, HttpClientService>();

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IRegistrationLogic, RegistrationLogic>();

// Homepage services
builder.Services.AddScoped<IHomePageLogic, HomePageLogic>();
builder.Services.AddScoped<IHomePageService, HomePageService>();

// Login services
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<ILoginLogic, LoginLogic>();

// Leaderboard services
builder.Services.AddHostedService<LeaderboardService>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .SetIsOriginAllowed((host) => true));
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Login/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}




app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();



app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();