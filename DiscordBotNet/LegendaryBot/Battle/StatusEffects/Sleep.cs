using DiscordBotNet.LegendaryBot.Battle.BattleEvents;
using DiscordBotNet.LegendaryBot.Battle.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Results;

namespace DiscordBotNet.LegendaryBot.Battle.StatusEffects;

public class Sleep: StatusEffect, IBattleEvent<CharacterDamageEventArgs>
{

    public override int MaxStacks => 1;

    public Sleep(Character caster) : base(caster)
    {
        
    }
    public override OverrideTurnType OverrideTurnType => OverrideTurnType.CannotMove;
    public override StatusEffectType EffectType => StatusEffectType.Debuff;

    public void OnBattleEvent(CharacterDamageEventArgs eventArgs, Character owner)
    {
        if(eventArgs.DamageResult.DamageReceiver != owner) return;
        if (eventArgs.DamageResult.StatusEffect is not null) return;
        owner.StatusEffects.Remove(this);
        owner.CurrentBattle.AdditionalTexts.Add($"{this} has been dispelled from {owner} due to an attack!");
    }

    public override UsageResult OverridenUsage(Character affected, ref Character target, ref BattleDecision decision, UsageType usageType)
    {
        affected.CurrentBattle.AdditionalTexts.Add($"{affected} is fast asleep");
        return new UsageResult(this)
        {
            UsageType = usageType,
            TargetType = TargetType.None,
            Text = "Snores...",
            User = affected
        };

    }
}