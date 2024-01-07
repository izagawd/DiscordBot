using System.Text.Json;
using DiscordBotNet.Database;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.Pages.characters.info;

public class InfoController : Controller
{
    public PostgreSqlContext DatabaseContext { get; private set; }

    public InfoController(PostgreSqlContext context)
    {
        DatabaseContext = context;
    }
    [HttpPost]
    public async Task<IActionResult> RemoveBlessingAsync([FromBody] string characterId)
    {
        var didParse = long.TryParse(characterId, out long characterGuid);
        if (!didParse) return Ok();
        var character = await DatabaseContext.Entity
            .OfType<Character>()
            .Where(i => i.Id == characterGuid)
            .Include(i => i.Blessing)
            .FirstOrDefaultAsync();
        if (character is null) return Ok();
        character.Blessing = null;
        await DatabaseContext.SaveChangesAsync();
        return Ok();
    }
    [HttpPost]
    public async Task<IActionResult> SetBlessingAsync([FromBody] JsonElement element)
    {
        var characterId = element.GetProperty("characterId").GetInt64();
        var blessingId = element.GetProperty("blessingId").GetInt64();

        var entityAnonymous = await DatabaseContext.UserData
            .FindOrCreateSelectAsync(User.GetDiscordUserId(),
                i =>
                    new
                    {
                        blessing = i.Inventory.OfType<Blessing>().FirstOrDefault(j =>j.Id == blessingId),
                        character = i.Inventory.OfType<Character>().FirstOrDefault(j => j.Id == characterId)
                    });


        var character = entityAnonymous.character;
        if (character is null) return Ok();
        var blessing = entityAnonymous.blessing;
        character.Blessing = blessing;
        if (blessing is not null && blessing.Character is not null)
        {
            blessing.CharacterId = null;
            blessing.Character.Blessing = null;
            blessing.Character = character;
        }
        await DatabaseContext.SaveChangesAsync();
        return Ok();
    }
}