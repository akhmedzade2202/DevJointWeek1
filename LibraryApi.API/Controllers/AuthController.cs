using LibraryApi.Application.DTOs.Auth;
using LibraryApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return Ok(result);
    }
    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var username = User.Identity?.Name;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        return Ok(new { username, role });
    }
}
