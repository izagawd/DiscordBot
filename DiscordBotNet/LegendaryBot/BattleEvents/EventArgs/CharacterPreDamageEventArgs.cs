using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;

public class CharacterPreDamageEventArgs : BattleEventArgs
{
    public DamageArgs DamageArgs { get; private set; }

    public CharacterPreDamageEventArgs(DamageArgs damageArgs)
    {
        DamageArgs = damageArgs;
    }
}