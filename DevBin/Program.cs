global using DevBin.Models;
using System.Net;
using DevBin.Data;
using DevBin.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var serverVersion = ServerVersion.AutoDetect(connectionString);
    options.UseMySql(connectionString, serverVersion);
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.Configure<SMTPConfig>(builder.Configuration.GetSection("SMTP"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddDefaultIdentity<ApplicationUser>((IdentityOptions options) =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
    options.Password = new PasswordOptions {
        RequireDigit = true,
        RequiredLength = 8,
        RequireLowercase = false,
        RequireUppercase = false,
        RequiredUniqueChars = 1,
        RequireNonAlphanumeric = false,
    };
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();

var authenticationBuilder = builder.Services.AddAuthentication()
    .AddGitHub(o => {
        o.ClientId = builder.Configuration["Authentication:GitHub:ClientID"];
        o.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"];
        o.SaveTokens = true;
    })
    /*.AddGitLab(o => {
        o.ClientId = builder.Configuration["Authentication:GitLab:ClientID"];
        o.ClientSecret = builder.Configuration["Authentication:GitLab:ClientSecret"];
    })*/
    .AddDiscord(o => {
        o.ClientId = builder.Configuration["Authentication:Discord:ClientID"];
        o.ClientSecret = builder.Configuration["Authentication:Discord:ClientSecret"];
        o.Scope.Add("identify");
        o.Scope.Add("email");
        o.SaveTokens = true;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();