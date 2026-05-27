using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddReverseProxy().LoadFromMemory(
[
    new RouteConfig { RouteId = "content", ClusterId = "content", Match = new RouteMatch { Path = "/api/v1/content/{**catch-all}" } },
    new RouteConfig { RouteId = "cta", ClusterId = "cta", Match = new RouteMatch { Path = "/api/v1/cta/{**catch-all}" } },
    new RouteConfig { RouteId = "auth", ClusterId = "auth", Match = new RouteMatch { Path = "/api/v1/auth/{**catch-all}" } },
    new RouteConfig { RouteId = "admin-content", ClusterId = "content", Match = new RouteMatch { Path = "/api/v1/admin/content/{**catch-all}" } }
],
[
    new ClusterConfig
    {
        ClusterId = "content",
        Destinations = new Dictionary<string, DestinationConfig>
        {
            ["content"] = new() { Address = "http://localhost:7001/" }
        }
    },
    new ClusterConfig
    {
        ClusterId = "cta",
        Destinations = new Dictionary<string, DestinationConfig>
        {
            ["cta"] = new() { Address = "http://localhost:7002/" }
        }
    },
    new ClusterConfig
    {
        ClusterId = "auth",
        Destinations = new Dictionary<string, DestinationConfig>
        {
            ["auth"] = new() { Address = "http://localhost:7003/" }
        }
    }
]);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "ApiGateway" }));
app.MapReverseProxy();

app.Run();
