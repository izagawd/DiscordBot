using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;

public class CharacterReviveEventArgs : BattleEventArgs
{
    public Character RevivedCharacter { get; }

    public CharacterReviveEventArgs(Character revivedCharacter)
    {
        RevivedCharacter = revivedCharacter;
    }
}