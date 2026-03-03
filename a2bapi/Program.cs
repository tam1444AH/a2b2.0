using System.Text;
using a2bapi.Data;
using a2bapi.Models;
using a2bapi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();


var frontendUrl = builder.Configuration["FrontendUrl"] ?? "http://localhost";
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(frontendUrl)
        .AllowAnyHeader()
        .AllowAnyMethod());
});


var connectionString = builder.Configuration.GetConnectionString("Default") ?? throw new Exception("Missing DB connection string.");
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFlightsService, FlightsService>();
builder.Services.AddScoped<IHotelsService, HotelsService>();
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.Configure<AviationStackSettings>(builder.Configuration.GetSection("AviationStack"));
builder.Services.AddHttpClient<IAviationStackService, AviationStackService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AviationStack:BaseUrl"] ?? "http://api.aviationstack.com/v1/");
});

builder.Services.Configure<AmadeusSettings>(builder.Configuration.GetSection("Amadeus"));
builder.Services.AddHttpClient<IAmadeusService, AmadeusService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Amadeus:BaseUrl"] ?? "https://test.api.amadeus.com/v1/");
});

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? throw new Exception("Missing Jwt config.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "Welcome to the A2B API!");

app.Run();