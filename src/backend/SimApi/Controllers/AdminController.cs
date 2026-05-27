using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimApi.Data;
using SimApi.DTOs;
using SimApi.Models;
using SimApi.Services;

namespace SimApi.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPasswordService _password;

    public AdminController(AppDbContext db, IPasswordService password)
    {
        _db = db;
        _password = password;
    }

    [HttpGet("organizations")]
    public async Task<ActionResult<List<OrganizationResponse>>> GetOrganizations()
    {
        var orgs = await _db.Organizations
            .Select(o => new OrganizationResponse
            {
                Id = o.Id,
                Name = o.Name,
                CreatedAt = o.CreatedAt,
                UserCount = o.Users.Count
            })
            .ToListAsync();

        return Ok(orgs);
    }

    [HttpPost("organizations")]
    public async Task<ActionResult<OrganizationResponse>> CreateOrganization(CreateOrganizationRequest request)
    {
        var org = new Organization { Name = request.Name };
        _db.Organizations.Add(org);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOrganizations), new OrganizationResponse
        {
            Id = org.Id,
            Name = org.Name,
            CreatedAt = org.CreatedAt,
            UserCount = 0
        });
    }

    [HttpPut("organizations/{id}")]
    public async Task<ActionResult> UpdateOrganization(Guid id, CreateOrganizationRequest request)
    {
        var org = await _db.Organizations.FindAsync(id);
        if (org == null) return NotFound();

        org.Name = request.Name;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("organizations/{id}")]
    public async Task<ActionResult> DeleteOrganization(Guid id)
    {
        var org = await _db.Organizations.FindAsync(id);
        if (org == null) return NotFound();

        _db.Organizations.Remove(org);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("organizations/{id}/users")]
    public async Task<ActionResult> GetOrganizationUsers(Guid id)
    {
        var users = await _db.Users
            .Where(u => u.OrganizationId == id)
            .Select(u => new
            {
                u.Id, u.Email, u.Name, Role = u.Role.ToString(), u.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost("organizations/{id}/users")]
    public async Task<ActionResult> InviteUser(Guid id, RegisterRequest request)
    {
        if (!await _db.Organizations.AnyAsync(o => o.Id == id))
            return NotFound("Organization not found");

        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            return Conflict("Email already registered");

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            return BadRequest("Invalid role");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = _password.Hash(request.Password),
            Name = request.Name,
            Role = role,
            OrganizationId = id
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new { user.Id, user.Email, user.Name, Role = user.Role.ToString() });
    }
}
