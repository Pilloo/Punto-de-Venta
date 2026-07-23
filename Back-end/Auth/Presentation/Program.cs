using System.Net;
using ErrorHandling.Extensions;
using AuthModule.Infrastructure.Persistence;
using AuthModule.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using AuthModule.Core.Extensions;
using AuthModule.Presentation.Grpc;
using Models.Auth;
using Serilog.Templates;
using Serilog.Enrichers.Span;
using Serilog.Sinks.OpenTelemetry;
using Services.CryptoService.Extensions;
using Services.CryptoService.Implementation;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

const string logTemplate = "[{@t:HH:mm:ss} {@l:u3}] {#if TraceId is not null}[Trace: {TraceId}] {#end}{@m}\n{@x}";
ExpressionTemplate formatter = new(logTemplate);

Log.Logger = new LoggerConfiguration()
             .MinimumLevel.Is(!builder.Environment.IsProduction()
                                  ? Serilog.Events.LogEventLevel.Debug
                                  : Serilog.Events.LogEventLevel.Information)
             .Enrich.FromLogContext()
             .Enrich.WithSpan()
             .WriteTo.Console(formatter)
             .WriteTo.OpenTelemetry(options => 
             {
                 options.Endpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
                 options.Protocol = OtlpProtocol.Grpc;
             })
             .WriteTo.File(formatter: formatter, path: "/app/logs/applog-.txt", rollingInterval: RollingInterval.Day)
             .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddCoreServices();
builder.Services.AddErrorHandlingService(builder.Configuration);
builder.Services.AddCryptoService();

builder.AddInfrastructureServices();

builder.Services.AddIdentityCore<User>().AddEntityFrameworkStores<AuthDbContext>().AddSignInManager()
       .AddDefaultTokenProviders();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddRouting(config => config.LowercaseUrls = true);

builder.Services.AddGrpc(options => options.EnableDetailedErrors = builder.Environment.IsDevelopment());
builder.Services.AddGrpcReflection();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
    {
        var jwtOptions = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromSeconds(30),
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

app.MapDefaultEndpoints();

app.MapGrpcService<AuthGrpcService>();

await app.SeedIdentityAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSerilogRequestLogging();
    app.MapGrpcReflectionService();
}

app.MapOpenApi();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();