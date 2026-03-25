using IdentityService.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add the connection string to configuration
builder.Configuration["ConnectionStrings:MongoDb"] = "mongodb://localhost:27017";

// Register our AuthService
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

public record AuthRequest(string Username, string Password);

// Register Endpoint
app.MapPost("/register", async (AuthService auth, AuthRequest request) => {
    var result = await auth.Register(request.Username, request.Password);
    return result != null ? Results.Ok(result) : Results.BadRequest("User exists");
});

// Login Endpoint
app.MapPost("/login", async (AuthService auth, AuthRequest request) => {
    var token = await auth.Login(request.Username, request.Password);
    return token != null ? Results.Ok(new { Token = token }) : Results.Unauthorized();
});

app.Run();