using DiscordBotNet.LegendaryBot.Battle.Results;

namespace DiscordBotNet.LegendaryBot.Battle.BattleEvents.EventArgs;

public class CharacterDamageEventArgs : System.EventArgs
{
    public DamageResult DamageResult { get;  }

    public CharacterDamageEventArgs(DamageResult damageResult)
    {
        DamageResult = damageResult;
    }
}