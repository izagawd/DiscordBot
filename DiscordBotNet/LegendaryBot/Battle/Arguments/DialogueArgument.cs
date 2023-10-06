using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Battle.Arguments;

public class DialogueArgument
{
    public DiscordColor CharacterColor { get; init; }

    public required string CharacterName { get;init; } 

    public required string CharacterUrl { get; init; }
    public required List<string> Dialogues { get; init; }

}