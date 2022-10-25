using System.Text;
using AuthProvider.Models;
using AuthProvider.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.Configure<UsersDatabaseSettings>(builder.Configuration.GetSection("Database"));

builder.Services.AddSingleton<UserService>();

var issuer = builder.Configuration.GetSection("Configuration")["Issuer"];
var secret = builder.Configuration.GetSection("Configuration")["Secret"];

builder.Services.AddAuthentication("OAuth")
    .AddJwtBearer("OAuth", config =>
    {
        var secretBytes = Encoding.UTF8.GetBytes(secret);
        var key = new SymmetricSecurityKey(secretBytes);
            
        config.Events = new JwtBearerEvents()
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Query.ContainsKey("access_token"))
                {
                    context.Token = context.Request.Query["access_token"];
                }
                            
                return Task.CompletedTask;
            }
        };
                    
        config.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidIssuer = issuer,
            ValidAudience = issuer,
            IssuerSigningKey = key,
        };
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();