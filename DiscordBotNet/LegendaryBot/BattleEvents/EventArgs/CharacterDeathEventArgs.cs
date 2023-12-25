using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;

public class CharacterDeathEventArgs : System.EventArgs
{
    public Character Killed { get;}

    public CharacterDeathEventArgs(Character killed)
    {
        Killed = killed;
    }
}