using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.Battle.BattleEvents.EventArgs;

public class CharacterDeathEventArgs : System.EventArgs
{
    public Character Killed { get;}

    public CharacterDeathEventArgs(Character killed)
    {
        Killed = killed;
    }
}