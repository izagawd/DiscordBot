using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Battle.Arguments;

public class DialogueArgument
{
    public DiscordColor CharacterColor { get; set; }

    public string CharacterName { get; set; } 

    public string CharacterUrl { get; set; }
    public List<string> Dialogues { get; set; }

}