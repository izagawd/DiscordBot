using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;

public class CharacterDamageEventArgs : System.EventArgs
{
    public DamageResult DamageResult { get;  }

    public CharacterDamageEventArgs(DamageResult damageResult)
    {
        DamageResult = damageResult;
    }
}