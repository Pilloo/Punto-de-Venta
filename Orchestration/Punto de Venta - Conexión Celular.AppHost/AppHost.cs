using Aspire.Hosting.Docker.Resources.ServiceNodes;
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
                             service.Restart = "on-failure:5";
                             service.Name = "auth-service";
                             service.AddVolume(new Volume
                             {
                                 Name = "auth-service-logs",
                                 Type = "bind",
                                 Source = "./logs",
                                 Target = "/app/logs",
                                 ReadOnly = false,
                             });
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
                              .WithHttpEndpoint(name: "http", port: 5016)
                              .WithHttpEndpoint(name: "grpc", port: 5017)
                              .WithReference(sqlInventoryDatabase)
                              .WithReference(authService)
                              .WaitFor(sqlInventoryDatabase)
                              .PublishAsDockerComposeService((_, service) =>
                              {
                                  service.Restart = "on-failure:5";
                                  service.Name = "inventory-service";
                                  service.AddVolume(new Volume
                                  {
                                      Name = "auth-service-logs",
                                      Type = "bind",
                                      Source = "./logs",
                                      Target = "/app/logs",
                                      ReadOnly = false,
                                  });
                              });

var inventoryServiceMigrations = inventoryService.AddEFMigrations("inventory-service-migration")
                                                 .WithMigrationsProject<Projects.Infrastructure>()
                                                 .WithReference(sqlInventoryDatabase)
                                                 .WaitFor(sqlInventoryDatabase)
                                                 .PublishAsMigrationBundle(publishContainer: true)
                                                 .PublishAsDockerComposeService((_, service) =>
                                                 {
                                                     service.Restart = "on-failure:5";
                                                 });


scalarDocs.WithApiReference(inventoryService).WaitForStart(inventoryService);

builder.Build().Run();