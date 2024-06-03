using AllSports.Helpers;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.Cookies;
using MVCAllSports.Helpers;
using MVCAllSports.Models;
using MVCAllSports.Services;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options => options.EnableEndpointRouting = false).AddSessionStateTempDataProvider();
builder.Services.AddSession();

builder.Services.AddSingleton<HelperMails>();

builder.Services.AddTransient<ServiceDeportes>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme =CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie();
builder.Services.AddControllersWithViews (options => options.EnableEndpointRouting = false);
builder.Services.AddHttpContextAccessor();

string jsonSecrets = await

    HelperSecretManager.GetSecretsAsync();

builder.Services.AddAWSService<IAmazonS3>();

KeysModel keysModel =

    JsonConvert.DeserializeObject<KeysModel>(jsonSecrets);

builder.Services.AddSingleton<KeysModel>(x => keysModel);
builder.Services.AddTransient<ServiceStorageAWS>();


string connectionCache = keysModel.CacheRedis;
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = connectionCache;
    options.InstanceName = "cache-allsports";
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.UseMvc(routes =>
{
    routes.MapRoute(
         name: "default",
    template: "{controller=Deportes}/{action=Index}/{id?}");
});

app.Run();
