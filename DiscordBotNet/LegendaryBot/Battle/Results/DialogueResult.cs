using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Battle.Results;

public class DialogueResult
{


    public bool TimedOut { get; init; } = false;
    public required DiscordMessage Message { get; init; }
    public bool Skipped { get; init; }= false;
}