using AspNetCoreRateLimit;
using FoosballProLeague.Webserver.BusinessLogic;
using FoosballProLeague.Webserver.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

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
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<ILoginLogic, LoginLogic>();

builder.Services.AddScoped<ITokenLogic, TokenLogic>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Configure Authentication
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.Cookie.Name = "accessToken"; // Same name as in your login method
        options.LoginPath = "/Login"; // Redirect to login if not authenticated
    });

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

// Add rate limiting middleware
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireRateLimiting("RatePolicy");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();