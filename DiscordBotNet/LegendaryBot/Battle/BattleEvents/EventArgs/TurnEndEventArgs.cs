using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.Battle.BattleEvents.EventArgs;

public class TurnEndEventArgs : System.EventArgs
{
    /// <summary>
    /// The character that the turn was ended with
    /// </summary>
    public Character Character { get;  }

    public TurnEndEventArgs(Character character)
    {
        Character = character;
    }
}