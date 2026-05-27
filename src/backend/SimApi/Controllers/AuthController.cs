using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimApi.Data;
using SimApi.DTOs;
using SimApi.Models;
using SimApi.Services;

namespace SimApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPasswordService _password;
    private readonly IJwtService _jwt;

    public AuthController(AppDbContext db, IPasswordService password, IJwtService jwt)
    {
        _db = db;
        _password = password;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            return Conflict("Email already registered");

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            return BadRequest("Invalid role. Use Admin, Instructor, or Trainee");

        var orgExists = await _db.Organizations.AnyAsync(o => o.Id == request.OrganizationId);
        if (!orgExists)
            return BadRequest("Organization not found");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = _password.Hash(request.Password),
            Name = request.Name,
            Role = role,
            OrganizationId = request.OrganizationId
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new AuthResponse
        {
            AccessToken = _jwt.GenerateAccessToken(user.Id, user.Email, user.Role.ToString(), user.OrganizationId),
            RefreshToken = _jwt.GenerateRefreshToken(),
            UserId = user.Id,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString(),
            OrganizationId = user.OrganizationId
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !_password.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Invalid email or password");

        user.RefreshToken = _jwt.GenerateRefreshToken();
        user.RefreshTokenExpires = DateTime.UtcNow.AddDays(7);
        await _db.SaveChangesAsync();

        return Ok(new AuthResponse
        {
            AccessToken = _jwt.GenerateAccessToken(user.Id, user.Email, user.Role.ToString(), user.OrganizationId),
            RefreshToken = user.RefreshToken,
            UserId = user.Id,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString(),
            OrganizationId = user.OrganizationId
        });
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.RefreshToken == request.RefreshToken &&
            u.RefreshTokenExpires > DateTime.UtcNow);

        if (user == null)
            return Unauthorized("Invalid or expired refresh token");

        user.RefreshToken = _jwt.GenerateRefreshToken();
        user.RefreshTokenExpires = DateTime.UtcNow.AddDays(7);
        await _db.SaveChangesAsync();

        return Ok(new AuthResponse
        {
            AccessToken = _jwt.GenerateAccessToken(user.Id, user.Email, user.Role.ToString(), user.OrganizationId),
            RefreshToken = user.RefreshToken,
            UserId = user.Id,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString(),
            OrganizationId = user.OrganizationId
        });
    }
}
