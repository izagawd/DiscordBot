using DiscordBotNet.Database;
using DiscordBotNet.Database.Models;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.Pages.team;


[Authorize]
public class Index : PageModel
{
    
    
    public UserData UserData { get; private set; }
    private PostgreSqlContext DatabaseContext { get;set; }

    public Index(PostgreSqlContext context)
    {
        DatabaseContext = context;
    }
    public async Task OnGetAsync()
    {
        UserData = await DatabaseContext.UserData.FindOrCreateAsync(User.GetDiscordUserId(), i =>
        i.Include(j => j.Inventory.Where(k => k is Character)));

        foreach (var i in UserData.Inventory.OfType<Player>())
        {
            await i.LoadAsync(User);
        }
    }


}