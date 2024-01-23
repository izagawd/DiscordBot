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
            .WithImageUrl("https://miro.medium.com/v2/resize:fit:721/1*9m0eBRsmz2-_in8K9_D_gA.png")
            .WithTitle("bruh")
            .WithDescription("bruh"));
    }

}