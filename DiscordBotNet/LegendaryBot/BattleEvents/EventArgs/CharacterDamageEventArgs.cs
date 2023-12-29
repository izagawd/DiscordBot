using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;

public class CharacterDamageEventArgs : BattleEventArgs
{
    public DamageResult DamageResult { get;  }

    public CharacterDamageEventArgs(DamageResult damageResult)
    {
        DamageResult = damageResult;
    }
}