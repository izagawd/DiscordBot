using DiscordBotNet.LegendaryBot.Battle;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.Pages.characters;

[Authorize]
public class Index : PageModel
{

    public Element? Element { get; set; }
    public string Name { get; set; }

    public string Sort { get; set; }
    public List<Character> Characters { get; set; }

    public PostgreSqlContext DatabaseContext { get; set; }
    public Index(PostgreSqlContext databaseContext)
    {
        DatabaseContext = databaseContext;
    }

    public async Task OnGetAsync(Element? element,string name, string sort)
    { 
        
       
        if (name is null)
        {
            name = "";
        }

        Name = name;
        if (sort is null)
        {
            sort = "level";
        }
        Sort = sort;
        Element = element;

        
        var userData = await DatabaseContext.UserData.FindOrCreateAsync(User.GetDiscordUserId(), i =>
            i.Include(j => j.Inventory.Where(k => k is Character)));

        Characters = userData.Inventory.OfType<Character>().ToList();

    }
}
