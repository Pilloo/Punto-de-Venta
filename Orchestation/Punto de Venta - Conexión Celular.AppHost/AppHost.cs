var builder = DistributedApplication.CreateBuilder(args);

var identityDb = builder.AddConnectionString("AuthDataBase");

var api = builder.AddProject<Projects.Presentation>("presentation").WithReference(identityDb);

var apiMigrations = api.AddEFMigrations("identity-api-migration", "AuthModule.Infrastructure.Persistence.AuthDbContext").WithMigrationsProject("..\\..\\Back-end\\Auth\\Infrastructure\\Infrastructure.csproj");

builder.Build().Run();
