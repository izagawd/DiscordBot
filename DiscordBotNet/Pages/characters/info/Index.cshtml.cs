using DiscordBotNet.Database;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
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
    public Blessing[] Blessings { get; private set; }

    public Index(PostgreSqlContext context)
    {
        DatabaseContext = context;
    }
    public async Task<IActionResult> OnGetAsync(string characterId)
    {
        var didParse = Guid.TryParse(characterId, out Guid id);
        if (!didParse) return Redirect("/characters");
        UserData = await DatabaseContext.UserData
            .Include(j => j.Inventory.Where(k => k is Blessing || k.Id == id))
            .Include(j => j.Inventory)
            .ThenInclude(i => (i as Blessing).Character)
            .Include(j => j.Inventory)
            .ThenInclude(j => (j as Character).Armor)
            .Include(j => j.Inventory)
            .ThenInclude(j => (j as Character).Boots)
            .Include(j => j.Inventory)
            .ThenInclude(j => (j as Character).Necklace)
            .Include(j => j.Inventory)
            .ThenInclude(j => (j as Character).Ring)
            .Include(j => j.Inventory)
            .ThenInclude(j => (j as Character).Weapon)
            .Include(j => j.Inventory)
            .ThenInclude(j => (j as Character).Helmet)
            .FindOrCreateAsync(User.GetDiscordUserId());
        Character = UserData.Inventory.OfType<Character>().FirstOrDefault(k => k.Id == id);
        Blessings = UserData.Inventory.OfType<Blessing>().ToArray();
        if (Character is null)
        {
            return Redirect("/characters");
        }

        Nullable<int> idk = 4;
        if (Character is Player player)
        {
            await player.LoadAsync(User);
        }
        else
        {
            await Character.LoadAsync();

        }
        return null;
    }
}