using DiscordBotNet.Database;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;

namespace DiscordBotNet.LegendaryBot.command;

public class Detail : BaseCommandClass
{
    [SlashCommand("detail", "Changes the color of the embed messages I send to you")]
    public async Task Execute(InteractionContext ctx,
        [Option("entity_id","the id of the thing you want to get details about")] string entity_id)
    {
        var embedBuilder = new DiscordEmbedBuilder()
            .WithUser(ctx.User)
            .WithTitle("Hmm")
            .WithDescription("Invalid id");
        if (!Guid.TryParse(entity_id, out Guid entityId))
        {
            var userColor = await DatabaseContext.UserData.FindOrCreateSelectAsync(ctx.User.Id, i => i.Color);
            embedBuilder.WithColor(userColor);
            await ctx.CreateResponseAsync(embedBuilder.Build());
            return;
        }
  
        var userData = await DatabaseContext.UserData
            .Include(i => i.Inventory.Where(j => j.Id == entityId))
            .FindOrCreateAsync(ctx.User.Id);
        embedBuilder.WithColor(userData.Color);
        if (!userData.Inventory.Any())
        {
            embedBuilder.WithDescription($"You do not have a blessing, gear or character with the id {entity_id}");
            await ctx.CreateResponseAsync(embedBuilder.Build());
            return;
        }

        var entity = userData.Inventory.First();
        if (entity is Player player)
        {
            await player.LoadAsync(ctx.User);
        }
        else
        {
            await entity.LoadAsync();
        }
        
        await using MemoryStream stream = new MemoryStream();
        await (await entity.GetDetailsImageAsync()).SaveAsPngAsync(stream);
        stream.Position = 0;
        embedBuilder.WithImageUrl("attachment://description.png");
        DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder()
            .AddFile("description.png", stream)
            .WithTitle("Detail")
            .AddEmbed(embedBuilder.Build());
        await ctx.CreateResponseAsync(builder);
   
    }
}