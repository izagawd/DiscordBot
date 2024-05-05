using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;

public class CharacterPostDamageEventArgs : BattleEventArgs
{ 
    public DamageResult DamageResult { get;  }
    public CharacterPostDamageEventArgs(DamageResult damageResult)
    {
        DamageResult = damageResult;
    }
}