
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Veterinaria.Data;
using Veterinaria.Interfaces;
using Veterinaria.Repositories;

var builder = WebApplication.CreateBuilder(args);

// MVC y Web API
builder.Services.AddControllersWithViews();

// Conexión a MySQL
var connectionString = builder.Configuration
    .GetConnectionString("VeterinariaConnection");

builder.Services.AddDbContext<VeterinariaContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    )
);

// Repositorios
builder.Services.AddScoped<IRepositorioCliente, RepositorioCliente>();
builder.Services.AddScoped<IRepositorioMascota, RepositorioMascota>();
builder.Services.AddScoped<IRepositorioEspecie, RepositorioEspecie>();
builder.Services.AddScoped<IRepositorioTurno, RepositorioTurno>();
builder.Services.AddScoped<IRepositorioRaza, RepositorioRaza>();
builder.Services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
builder.Services.AddScoped<IRepositorioConsulta, RepositorioConsulta>();
builder.Services.AddScoped<IRepositorioVeterinario, RepositorioVeterinario>();
builder.Services.AddScoped<IRepositorioHClinica, RepositorioHClinica>();

// Clave JWT almacenada fuera del código fuente
var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey) ||
    Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "Debe configurar Jwt:Key con al menos 32 bytes."
    );
}

// Autenticación: Cookies para MVC y JWT para API
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)

    .AddCookie(options =>
    {
        options.LoginPath = "/Usuario/Login";
    })

    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = "VeterinariaGit",
            ValidAudience = "VeterinariaGit.Api",

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            ),

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Rutas MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
