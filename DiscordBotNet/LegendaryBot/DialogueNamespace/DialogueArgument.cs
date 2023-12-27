using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.DialogueNamespace;

public abstract class DialogueArgument
{
    public required DialogueProfile Profile { get; init; }
    public DiscordColor CharacterColor => Profile.CharacterColor;

    public string CharacterName => Profile.CharacterName;

    public string CharacterUrl => Profile.CharacterUrl;
}