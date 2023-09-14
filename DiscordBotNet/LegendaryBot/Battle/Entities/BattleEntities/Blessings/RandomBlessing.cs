using DiscordBotNet.LegendaryBot.Battle.BattleEvents;
using DiscordBotNet.LegendaryBot.Battle.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Blessings;

public class RandomBlessing : Blessing, IBattleEvent<CharacterDamageEventArgs>
{
    public void OnEvent(CharacterDamageEventArgs eventArgs, Character owner)
    {
        if (eventArgs.DamageResult.DamageReceiver != owner) return;

        owner.RecoverHealth((int)0.05 * owner.MaxHealth);

    }
}