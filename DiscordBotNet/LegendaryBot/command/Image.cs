using DiscordBotNet.Extensions;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotNet.LegendaryBot.command;

public class Image : BaseCommandClass
{
    [SlashCommand("image", "Begin your journey by playing the tutorial!")]
    public async Task Execute(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(new DiscordEmbedBuilder()
            .WithUser(ctx.User)
            .WithTitle("IDK")
            .WithImageUrl($"{Website.DomainName}/battle_images/battle.png"));

    }

}