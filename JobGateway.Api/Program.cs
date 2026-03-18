using Yarp.ReverseProxy;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// 1. Define the Rate Limiting Policy
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(policyName: "customPolicy", opt =>
    {
        opt.PermitLimit = 5;            // Max 5 requests
        opt.Window = TimeSpan.FromSeconds(10); // Per 10 seconds
        opt.QueueLimit = 0;             // Don't queue extra requests, just reject them
    });

    // What happens when someone is blocked?
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// 2. Use the Rate Limiter middleware
app.UseRateLimiter();

app.MapReverseProxy();

app.MapGet("/", () => "Gateway is running!");

app.Run();