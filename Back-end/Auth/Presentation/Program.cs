using ErrorHandling.Extensions;
using AuthModule.Infrastructure.Persistence;
using AuthModule.Infrastructure.Extensions;
using AuthModule.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Models;
using Serilog;
using System.Security.Cryptography;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Identity;
using AuthModule.Core.Extensions;
using AuthModule.Core.Interfaces;
using Serilog.Templates;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog.Enrichers.Span;

var builder = WebApplication.CreateBuilder(args);

const string logTemplate = "[{@t:HH:mm:ss} {@l:u3}] {#if TraceId is not null}[Trace: {TraceId}] {#end}{@m}\n{@x}";
ExpressionTemplate formatter = new(logTemplate);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Is(!builder.Environment.IsProduction()
        ? Serilog.Events.LogEventLevel.Debug
        : Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithSpan()
    .WriteTo.Console(formatter)
    .WriteTo.File(formatter: formatter, path: "Logs/applog-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

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
                CryptoService cryptoService = new();
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
