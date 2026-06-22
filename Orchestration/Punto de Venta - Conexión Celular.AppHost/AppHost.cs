using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

// ### General Configurations ###

builder.AddDockerComposeEnvironment("aspire");

var scalarDocs = builder.AddScalarApiReference(options =>
{
    options
        .PreferHttpsEndpoint()
        .AllowSelfSignedCertificates();
});

var sqlPassword = builder.AddParameter("sql-password", secret: true);

var sqlServer = builder.AddSqlServer(name: "sql-server", password: sqlPassword, port: 1433).WithDataVolume();

// ### Auth Service Configurations ###

var sqlAuthDatabase = sqlServer.AddDatabase(name: "auth-database", databaseName: "auth_database");

var authService = builder.AddProject<Projects.Auth>("auth")
    .WithHttpEndpoint(name: "http", port: 5014)
    .WithHttpEndpoint(name: "grpc", port: 5015)
    .WithReference(sqlAuthDatabase)
    .WaitFor(sqlAuthDatabase)
    .PublishAsDockerComposeService((_, service) =>
    {
        service.Name = "auth-service";
    });

scalarDocs.WithApiReference(authService).WaitForStart(authService);

var authServiceMigrations = authService.AddEFMigrations("auth-service-migration")
    .WithMigrationsProject<Projects.Infrastructure>()
    .WithReference(sqlAuthDatabase)
    .WaitFor(sqlAuthDatabase)
    .PublishAsMigrationBundle(publishContainer: true)
    .PublishAsDockerComposeService((_, service) =>
    {
        service.Restart = "on-failure:5";
    });

// ### Inventory Service Configurations ###

var sqlInventoryDatabase = sqlServer.AddDatabase(name: "inventory-database", databaseName: "inventory_database");

var inventoryService = builder.AddProject<Projects.Inventory>("inventory")
    .WithReference(sqlInventoryDatabase)
    .WaitFor(sqlInventoryDatabase)
    .PublishAsDockerComposeService((_, service) =>
    {
        service.Restart = "on-failure:5";
    });

scalarDocs.WithApiReference(inventoryService).WaitForStart(inventoryService);

builder.Build().Run();
