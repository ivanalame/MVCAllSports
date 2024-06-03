using AllSports.Helpers;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.Cookies;
using MVCAllSports.Helpers;
using MVCAllSports.Models;
using MVCAllSports.Services;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Cargar secretos desde el administrador de secretos
string jsonSecrets = await HelperSecretManager.GetSecretsAsync();
KeysModel keysModel = JsonConvert.DeserializeObject<KeysModel>(jsonSecrets);

// Configurar servicios de AWS y S3
builder.Services.AddAWSService<IAmazonS3>();

// Registrar KeysModel como un singleton
builder.Services.AddSingleton<KeysModel>(x => keysModel);

// Configurar Redis Cache
string connectionCache = keysModel.CacheRedis;
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = connectionCache;
    options.InstanceName = "cache-allsports";
});

// Configurar servicios de controlador con soporte para el estado de la sesión
builder.Services.AddControllersWithViews(options => options.EnableEndpointRouting = false)
                .AddSessionStateTempDataProvider();

// Configurar sesión
builder.Services.AddSession();

// Registrar servicios personalizados
builder.Services.AddSingleton<HelperMails>();
builder.Services.AddTransient<ServiceStorageAWS>();
builder.Services.AddTransient<ServiceDeportes>();

// Configurar autenticación con cookies
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie();

// Añadir soporte para HttpContextAccessor
builder.Services.AddHttpContextAccessor();


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
