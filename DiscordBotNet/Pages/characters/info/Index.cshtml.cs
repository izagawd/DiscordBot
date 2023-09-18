using DiscordBotNet.Database;
using DiscordBotNet.Database.Models;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Blessings;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.Pages.characters.info;
[Authorize]
public class Index : PageModel
{
    public PostgreSqlContext DatabaseContext { get; private set; }
    public Character Character { get; private set; }
    public UserData UserData { get; private set; }
    public List<Blessing> Blessings { get; private set; }

    public Index(PostgreSqlContext context)
    {
        DatabaseContext = context;
    }
    public async Task<IActionResult> OnGetAsync(string characterId)
    {
        var didParse = Guid.TryParse(characterId, out Guid id);
        if (!didParse) return Redirect("/characters");
        UserData = await DatabaseContext.UserData.FindOrCreateAsync(User.GetDiscordUserId(), i =>
            i.Include(j => j.Inventory.Where(k => k is Blessing || k is Character)));
        Character = UserData.Inventory.OfType<Character>().FirstOrDefault(k => k.Id == id);
        Blessings = UserData.Inventory.OfType<Blessing>().ToList();
        if (Character is null)
        {
            return Redirect("/characters");
        }

        Nullable<int> idk = 4;
        if (Character is Player player)
        {
            await player.LoadAsync(User);
        }
        return null;
    }
}