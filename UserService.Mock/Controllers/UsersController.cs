using Microsoft.AspNetCore.Mvc;
using UserService.Mock.Models;

namespace UserService.Mock.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private static readonly List<User> _users = new()
    {
        new User
        {
            Id = "1",
            Username = "john.doe",
            Email = "john.doe@example.com",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+1234567890",
            PasswordHash = "password123", // In real app, this would be hashed
            TwoFactorEnabled = true
        },
        new User
        {
            Id = "2",
            Username = "jane.smith",
            Email = "jane.smith@example.com",
            FirstName = "Jane",
            LastName = "Smith",
            PhoneNumber = "+1234567891",
            PasswordHash = "password456", // In real app, this would be hashed
            TwoFactorEnabled = false
        },
        new User
        {
            Id = "3",
            Username = "admin",
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "User",
            PhoneNumber = "+1234567892",
            PasswordHash = "admin123", // In real app, this would be hashed
            TwoFactorEnabled = true
        }
    };

    [HttpPost("validate")]
    public IActionResult ValidateUser([FromBody] ValidateUserRequest request)
    {
        var user = _users.FirstOrDefault(u => 
            u.Username == request.Username && u.PasswordHash == request.Password);

        if (user == null)
        {
            return Unauthorized();
        }

        // Return user without password hash
        var userResponse = new User
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            TwoFactorEnabled = user.TwoFactorEnabled
        };

        return Ok(userResponse);
    }

    [HttpGet("{id}")]
    public IActionResult GetUser(string id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        
        if (user == null)
        {
            return NotFound();
        }

        // Return user without password hash
        var userResponse = new User
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            TwoFactorEnabled = user.TwoFactorEnabled
        };

        return Ok(userResponse);
    }

    [HttpGet]
    public IActionResult GetAllUsers()
    {
        var usersResponse = _users.Select(user => new User
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            TwoFactorEnabled = user.TwoFactorEnabled
        }).ToList();

        return Ok(usersResponse);
    }
}
