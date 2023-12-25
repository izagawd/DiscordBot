using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;

public class TurnStartEventArgs : System.EventArgs
{
    /// <summary>
    /// The character that the turn was started with
    /// </summary>
    public Character Character { get;  }

    public TurnStartEventArgs(Character character)
    {
        Character = character;
    }
}