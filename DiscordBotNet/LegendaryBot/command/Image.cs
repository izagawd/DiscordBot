using DiscordBotNet.Database;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotNet.LegendaryBot.command;

public class Image : BaseCommandClass
{
    [SlashCommand("image", "Begin your journey by playing the tutorial!")]
    public async Task Execute(InteractionContext ctx)
    {
        PostgreSqlContext ctxx = new PostgreSqlContext();
        UserData user = await ctxx.UserData
            .IncludeTeamWithAllEquipments()
            .FindOrCreateAsync(ctx.User.Id);
        Character character = user.CharacterTeamArray.First();
        await character.LoadAsync();
        Image<Rgba32> image = await character.GetDetailsImageAsync();
        var stream = new MemoryStream();
        await image.SaveAsPngAsync(stream);
        stream.Position = 0;
        DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            .WithUser(ctx.User)
            .WithTitle("idk bro")
            .WithImageUrl("attachment://idk.png");
        await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
            .WithTitle("idk bro")
            .AddFile("idk.png", stream)
            .AddEmbed(builder));

    }

}