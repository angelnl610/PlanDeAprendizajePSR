using Microsoft.AspNetCore.Authentication.Cookies;
using PizzaPlanetaBiblioteca.IServices;
using PizzaPlanetServicios.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// Autenticación por Cookies estándar
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login";
        options.LogoutPath = "/Cuenta/Logout";
        options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

// Configuración de URL base y servicios HTTP
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5272";
var apiUri = new Uri(apiBaseUrl);

builder.Services.AddHttpClient<IAuthService, AuthService>(c => c.BaseAddress = apiUri);
builder.Services.AddHttpClient<IPizzaService, PizzaService>(c => c.BaseAddress = apiUri);
builder.Services.AddHttpClient<IPedidoService, PedidoService>(c => c.BaseAddress = apiUri);
builder.Services.AddHttpClient<IUsuarioService, UsuarioService>(c => c.BaseAddress = apiUri);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
