using IdentityService.Api.Models;
using MongoDB.Driver;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityService.Api.Services;

public class AuthService
{
    private readonly IMongoCollection<User> _users;
    private readonly string _jwtSecret = "YourSuperSecretKey123!_DoNotShare"; // Match this with Gateway!

    public AuthService(IConfiguration config)
    {
        var client = new MongoClient(config.GetConnectionString("MongoDb"));
        var database = client.GetDatabase("IdentityDb");
        _users = database.GetCollection<User>("Users");
    }

    public async Task<string?> Register(string username, string password)
    {
        var existingUser = await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
        if (existingUser != null) return null;

        var user = new User {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        await _users.InsertOneAsync(user);
        return "User registered successfully";
    }

    public async Task<string?> Login(string username, string password)
    {
        var user = await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        return GenerateJwtToken(user);
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSecret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.Username) }),
            Expires = DateTime.UtcNow.AddHours(2),
            Issuer = "JobGateway",
            Audience = "JobGatewayUsers",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}