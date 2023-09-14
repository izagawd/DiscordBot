using DiscordBotNet.LegendaryBot.Database;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotNet.LegendaryBot.command;

public class Avatar: BaseCommandClass
{
    public override BotCommandType BotCommandType { get; } = BotCommandType.Other;

    [SlashCommand("avatar", "Displays your avatar, or someone elses avatar")]
    public async Task Execute(InteractionContext ctx, [Option("user", "User to challenge")] DiscordUser? user = null)
    {
        if(user is null)
        {
            user = ctx.User;
        }
  

        DiscordColor color = await DatabaseContext.UserData
            .FindOrCreateSelectAsync(user.Id, i => i.Color);
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithTitle($"**{user.Username}'s avatar**")
            .WithAuthor(ctx.User.Username, iconUrl: ctx.User.AvatarUrl)
            .WithColor(color)
            .WithImageUrl(user.AvatarUrl)
            .WithTimestamp(DateTime.Now);
        await ctx.CreateResponseAsync(embed);
    }
}