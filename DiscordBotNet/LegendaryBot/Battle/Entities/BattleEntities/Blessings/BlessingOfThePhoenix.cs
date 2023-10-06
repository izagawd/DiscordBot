using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Battle.BattleEvents;
using DiscordBotNet.LegendaryBot.Battle.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Blessings;

public class BlessingOfThePhoenix : Blessing, IBattleEvent<CharacterDamageEventArgs>
{
    public int GetHealthPercentRecovering(int level)
    {
        if (level >= 15) return 10;
        if (level >= 12) return 9;
        if (level >= 9) return 8;
        if (level >= 6) return 7;
        if (level >= 3) return 6;
        return 5;
    }
    public void OnBattleEvent(CharacterDamageEventArgs eventArgs, Character owner)
    {
        if (eventArgs.DamageResult.DamageReceiver != owner) return;

         owner.RecoverHealth((GetHealthPercentRecovering(Level) *  0.01 * owner.MaxHealth).Round(),$"{owner} recovered $ health via the blessing of the phoenix");
  

    }
}