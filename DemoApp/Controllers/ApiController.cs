using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DemoApp.Controllers;

[ApiController]
[Route("api")]
public class ApiController : ControllerBase
{
    [HttpGet("public")]
    public IActionResult GetPublicData()
    {
        return Ok(new
        {
            message = "This is public data - no authentication required",
            timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("protected")]
    [Authorize]
    public IActionResult GetProtectedData()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var scopes = User.FindAll("scope").Select(c => c.Value).ToList();

        return Ok(new
        {
            message = "This is protected data - authentication required",
            user = new
            {
                id = userId,
                username = username,
                email = email,
                scopes = scopes
            },
            timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("admin")]
    [Authorize]
    public IActionResult GetAdminData()
    {
        var scopes = User.FindAll("scope").Select(c => c.Value).ToList();
        
        if (!scopes.Contains("admin"))
        {
            return Forbid("Admin scope required");
        }

        return Ok(new
        {
            message = "This is admin data - admin scope required",
            timestamp = DateTime.UtcNow
        });
    }
}
