var builder = DistributedApplication.CreateBuilder(args);

var keycloak = builder.AddKeycloak("keycloak", 8080)
                      .WithDataVolume()
                      .WithImageTag("latest")
                      .WithRealmImport("../realms");

var apiService = builder.AddProject<Projects.AspireKeycloak_ApiService>("apiservice")
                        .WithReference(keycloak);

builder.AddProject<Projects.AspireKeycloak_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(keycloak)
    .WithReference(apiService);

builder.Build().Run();

