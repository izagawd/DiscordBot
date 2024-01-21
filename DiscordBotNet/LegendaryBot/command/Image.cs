using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotNet.LegendaryBot.command;

public class Image : BaseCommandClass
{
    [SlashCommand("image", "Begin your journey by playing the tutorial!")]
    public async Task Execute(InteractionContext ctx)
    {
        var lily = new Lily();
        await ctx.CreateResponseAsync(new DiscordEmbedBuilder()
            .WithImageUrl(lily.ImageRepresentation)
            .WithTitle("bruh")
            .WithDescription("bruh"));
    }

}