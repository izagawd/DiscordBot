using Microsoft.AspNetCore.Mvc;

namespace DiscordBotNet.Pages;

public class AccountController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}