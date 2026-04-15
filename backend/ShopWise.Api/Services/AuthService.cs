using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShopWise.Api.Data;
using ShopWise.Api.DTOs;
using ShopWise.Api.Models;

namespace ShopWise.Api.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    private static string HashPassword(string password)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            return null;

        var user = new User
        {
            Email = req.Email,
            PasswordHash = HashPassword(req.Password),
            FullName = req.FullName,
            Role = "customer"
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _db.Carts.Add(new Cart { UserId = user.Id });
        await _db.SaveChangesAsync();

        return new AuthResponse(GenerateToken(user), user.Email, user.FullName, user.Role);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest req)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == req.Email);
        if (user is null || user.PasswordHash != HashPassword(req.Password))
            return null;

        return new AuthResponse(GenerateToken(user), user.Email, user.FullName, user.Role);
    }

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
