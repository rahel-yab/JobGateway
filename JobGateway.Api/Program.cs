using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

// 1. Add YARP services and tell it to load config from appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// 2. Map the proxy to the request pipeline
app.MapReverseProxy();

app.Run();