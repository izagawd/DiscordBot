using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Battle.Results;

public class DialogueResult
{


    public bool TimedOut { get; set; } = false;
    public DiscordMessage Message { get; set; }
    public bool Skipped { get; set; }= false;
}