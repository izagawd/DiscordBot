using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.Pages.team;

public class TeamController : Controller
{
    private PostgreSqlContext DatabaseContext { get; }
    public TeamController(PostgreSqlContext databaseContext)
    {
        DatabaseContext = databaseContext;
    }
    [HttpPost]
    public async Task<IActionResult> RemoveFromTeamAsync([FromBody]string idString)
    {
        Guid id = Guid.Parse(idString);
        var UserData = await DatabaseContext.UserData.FindOrCreateAsync(User.GetDiscordUserId(), i => i.IncludeTeam());

        var userTeam = UserData.Team;
        if (userTeam.Count <= 1) return Ok();
        var characterToRemove = userTeam.FirstOrDefault(i => i.Id == id);
        
        if (characterToRemove is not null)
        {
            UserData.RemoveFromTeam(characterToRemove);
        }
        
        await DatabaseContext.SaveChangesAsync();

        return Ok();
    }
    [HttpPost]
    public async Task<IActionResult> AddToTeamAsync([FromBody]string idString)
    {
        Guid id = Guid.Parse(idString);
        var userData = await DatabaseContext.UserData.FindOrCreateAsync(User.GetDiscordUserId(), i => 
            i.Include(j => j.Inventory.Where(k => k.Id == id)).IncludeTeam());
        var character = userData.Inventory.OfType<Character>().FirstOrDefault(i => i.Id == id);
        
        if (character is not null)
        {
            userData.AddToTeam(character);
        }
        await DatabaseContext.SaveChangesAsync();
        return Ok();
    }

}