using System.Text.Json;
using DiscordBotNet.Database;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Blessings;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
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
        var didParse = Guid.TryParse(characterId, out Guid characterGuid);
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
        var characterId = element.GetProperty("characterId").ToString();
        var blessingId = element.GetProperty("blessingId").ToString();
        var didParse = Guid.TryParse(characterId, out Guid characterGuid);
       var anotherParse =  Guid.TryParse(blessingId, out Guid blessingGuid);
      
        if (!(didParse && anotherParse)) return Ok();
        var entityArray = await DatabaseContext.Entity
            .Where(i => ((i is Blessing || i is Character) && (i.Id == characterGuid || i.Id == blessingGuid)) || (i is Character && (i as Character).BlessingId == blessingGuid) && i.UserDataId == User.GetDiscordUserId())
            .ToArrayAsync();

        var character = entityArray.OfType<Character>().FirstOrDefault();
        if (character is null) return Ok();
        var blessing = entityArray.OfType<Blessing>().FirstOrDefault();
        character.Blessing = blessing;
        if (blessing is not null && blessing.Character is not null)
        {
            blessing.Character.BlessingId = null;
            blessing.Character.Blessing = null;
            blessing.Character = character;
        }
        await DatabaseContext.SaveChangesAsync();
        return Ok();
    }
}