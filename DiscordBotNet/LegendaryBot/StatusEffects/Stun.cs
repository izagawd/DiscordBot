using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class Stun : StatusEffect
{
    public override string Description { get; } = "Makes affected not able to move";
    public override bool IsRenewable => true;
    public override bool HasLevels => false;

    public Stun(Character caster) : base( caster)
    {
        
    }


    public override StatusEffectType EffectType => StatusEffectType.Debuff;

    public override int MaxStacks => 1;
    public override OverrideTurnType OverrideTurnType => OverrideTurnType.CannotMove;

    public override UsageResult OverridenUsage(Character affected,ref Character target, ref BattleDecision decision, UsageType usageType)
    {
        decision = BattleDecision.Other;
        affected.CurrentBattle.AddAdditionalText($"{affected} cannot move because they are stunned!");
        return new UsageResult(this)
        {
            Text = "dizzy...",
            TargetType = TargetType.None,
            UsageType = usageType,
            User = affected
        };
    }
}