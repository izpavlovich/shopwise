using Microsoft.AspNetCore.Mvc;
using ShopWise.Api.DTOs;
using ShopWise.Api.Services;

namespace ShopWise.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;

    public AuthController(AuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        var result = await _auth.RegisterAsync(req);
        if (result is null)
            return Conflict(new { message = "Email already in use." });
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var result = await _auth.LoginAsync(req);
        if (result is null)
            return Unauthorized(new { message = "Invalid credentials." });
        return Ok(result);
    }
}
