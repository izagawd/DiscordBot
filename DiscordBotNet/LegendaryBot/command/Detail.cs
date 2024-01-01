using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.command;

public class Detail : BaseCommandClass
{
    [SlashCommand("detail", "Changes the color of the embed messages I send to you")]
    public async Task Execute(InteractionContext ctx,
        [Option("entity_id","the id of the thing you want to get details about")] long entityId)
    {
        var embedBuilder = new DiscordEmbedBuilder()
            .WithUser(ctx.User)
            .WithTitle("Hmm")
            .WithDescription("Invalid id");

  
        var userData = await DatabaseContext.UserData
            .Include(i => i.Inventory.Where(j => j.Id == entityId))
            .FindOrCreateAsync((long)ctx.User.Id);
        embedBuilder.WithColor(userData.Color);
        if (!userData.Inventory.Any())
        {
            embedBuilder.WithDescription($"You do not have anything with the id {entityId}");
            await ctx.CreateResponseAsync(embedBuilder);
            return;
        }

        var entity = userData.Inventory.First();
        if (entity is Player player)
            await player.LoadAsync(ctx.User);
        else
            await entity.LoadAsync();
        
        await using var stream = new MemoryStream();
        await (await entity.GetDetailsImageAsync()).SaveAsPngAsync(stream);
        stream.Position = 0;
        embedBuilder.WithImageUrl("attachment://description.png");

        embedBuilder
            .WithTitle("Here you go!")
            .WithDescription($"Name: {entity.Name}");
        DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder()
            .AddFile("description.png", stream)
            .WithTitle("Detail")
            .AddEmbed(embedBuilder.Build());
        await ctx.CreateResponseAsync(builder);
   
    }
}