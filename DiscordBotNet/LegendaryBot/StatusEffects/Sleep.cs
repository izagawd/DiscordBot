using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class Sleep: StatusEffect
{
    public override string Description { get; } =
        "Makes affected not able to move. Is dispelled when affected takes damage from a move";
    public override int MaxStacks => 1;

    public Sleep(Character caster) : base(caster)
    {
        
    }
    public override OverrideTurnType OverrideTurnType => OverrideTurnType.CannotMove;
    public override StatusEffectType EffectType => StatusEffectType.Debuff;

    public override void OnBattleEvent(BattleEventArgs eventArgs, Character owner)
    {
        if(eventArgs is not CharacterDamageEventArgs damageEventArgs) return;
        if(damageEventArgs.DamageResult.DamageReceiver != owner) return;
        if (damageEventArgs.DamageResult.StatusEffect is not null) return;
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