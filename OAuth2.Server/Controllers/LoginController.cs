using Microsoft.AspNetCore.Mvc;

namespace OAuth2.Server.Controllers;

[Controller]
public class LoginController : Controller
{
    [HttpGet("login")]
    public IActionResult Index()
    {
        return View();
    }
}
