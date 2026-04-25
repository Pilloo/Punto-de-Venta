using Core.Extensions;
using Core.Interfaces;
using ErrorHandling.Extensions;
using Infrastructure.Data;
using Infrastructure.Extensions;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Models;
using Serilog;
using System.Security.Cryptography;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
builder.Services.AddCoreServices();
builder.Services.AddErrorHadlingService(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddIdentityCore<User>().AddEntityFrameworkStores<AuthDbContext>().AddSignInManager().AddDefaultTokenProviders();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddRouting(config => config.LowercaseUrls = true);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(
    options =>
    {
        var jwtOptions = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.GetValue<string>("Issuer"),
            ValidAudience = jwtOptions.GetValue<string>("Audience"),
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                ICryptoService cryptoService = new CryptoService();
                ECDsa key = cryptoService.LoadEcdsaKey(jwtOptions.GetValue<string>("PublicKeyPath")!);
                return [new ECDsaSecurityKey(key)];
            },
        };
    }
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSerilogRequestLogging();
    app.MapOpenApi();
    app.MapScalarApiReference();
}


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
