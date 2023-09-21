using DiscordBotNet.Database;
using DiscordBotNet.LegendaryBot.Battle.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscordBotNet.Pages.add;
[Authorize]
public class Index(PostgreSqlContext context) : PageModel
{
    public PostgreSqlContext DatabaseContext { get; private set; } = context;

    public async Task OnPostAsync(string type)
    {
        Type theType = Bot.AllAssemblyTypes.First(i => i.Name == type);
        Entity entity =(Entity) Activator.CreateInstance(theType);
        entity.UserDataId = User.GetDiscordUserId();
        await DatabaseContext.Entity.AddAsync(entity);
        await DatabaseContext.SaveChangesAsync();
    }
}