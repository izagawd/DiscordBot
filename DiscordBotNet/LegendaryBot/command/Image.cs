using DiscordBotNet.Database;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using SixLabors.ImageSharp.PixelFormats;

namespace DiscordBotNet.LegendaryBot.command;

public class Image : BaseCommandClass
{
    [SlashCommand("image", "Begin your journey by playing the tutorial!")]
    public async Task Execute(InteractionContext ctx)
    {

    }

}