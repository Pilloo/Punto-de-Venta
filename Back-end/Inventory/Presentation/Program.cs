using System.Security.Cryptography;
using ErrorHandling.Extensions;
using Inventory.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Inventory.Core.Extensions;
using Inventory.Presentation.Grpc;
using Serilog.Templates;
using Serilog.Enrichers.Span;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpClientDefaults(http => { http.AddServiceDiscovery(); });

builder.Services.AddServiceDiscovery();
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
             .WriteTo.File(formatter: formatter, path: "Logs/applog-.txt", rollingInterval: RollingInterval.Day)
             .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddCoreServices();
builder.Services.AddErrorHadlingService(builder.Configuration);

builder.AddInfrastructureService();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddRouting(config => config.LowercaseUrls = true);
builder.Services.AddGrpc(options => options.EnableDetailedErrors = builder.Environment.IsDevelopment());

builder.Services.AddGrpcReflection();

builder.Services.AddRequestTimeouts();

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
                string fileContent = File.ReadAllText(jwtOptions.GetValue<string>("PublicKeyPath")!);
                ECDsa key = ECDsa.Create();

                key.ImportFromPem(fileContent);

                return [new ECDsaSecurityKey(key)];
            },
        };
    }
);

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGrpcService<ProductsGrpcService>();

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

app.UseRequestTimeouts();

app.MapControllers();

app.Run();