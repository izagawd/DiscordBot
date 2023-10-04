﻿using DiscordBotNet.Database;
using DiscordBotNet.LegendaryBot;
using DiscordBotNet.LegendaryBot.Battle.Entities;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Blessings;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Entities.Gears;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscordBotNet.Pages.add;
[Authorize]
public class Index : PageModel
{
    public Index(PostgreSqlContext context)
    {
        DatabaseContext = context;
    }
    public PostgreSqlContext DatabaseContext { get; private set; }

    public async Task OnPostAsync(string type)
    {
        Console.WriteLine(type);
        Type theType = Bot.AllAssemblyTypes.First(i => i.Name == type);
        Entity entity =(Entity) Activator.CreateInstance(theType)!;
        entity.UserDataId = User.GetDiscordUserId();
        await DatabaseContext.Entity.AddAsync(entity);
        await DatabaseContext.SaveChangesAsync();
    }
    /// <summary>
    /// Categorizes an entity into blessing, gear, or character
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>The type it is categorized as</returns>
    public Type Categorize(Type entityType)
    {
        if (entityType.IsRelatedToType(typeof(Gear))) return typeof(Gear);
        if (entityType.IsRelatedToType(typeof(Character))) return typeof(Character);
        if (entityType.IsRelatedToType(typeof(Blessing))) return typeof(Blessing);
        return typeof(Entity);
    }
}