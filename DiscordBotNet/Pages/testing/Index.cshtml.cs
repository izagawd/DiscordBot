using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscordBotNet.Pages.testing;

public class Index : PageModel
{
    public int Id { get; private set; }
    public int? Idk { get; set; } = null;
    public void OnGet()
    {
        Console.WriteLine(Id);
        Idk = 90;
        TempData["idk"] = Idk;
    }
    public void OnPost()
    {
        Idk = TempData["idk"] as int?;
        Console.WriteLine(Idk);
        Idk += 1;
        TempData["idk"] = Idk;
    }
}