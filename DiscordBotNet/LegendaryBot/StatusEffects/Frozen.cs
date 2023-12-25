using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class Frozen : StatusEffect
{
    public override bool IsRenewable => true;
    public override bool HasLevels => false;

    public Frozen(Character caster) : base( caster)
    {
    }


    public override StatusEffectType EffectType => StatusEffectType.Debuff;

    public override int MaxStacks => 1;
    public override OverrideTurnType OverrideTurnType => OverrideTurnType.CannotMove;

    public override UsageResult OverridenUsage(Character affected,ref Character target, ref BattleDecision decision, UsageType usageType)
    {
        decision = BattleDecision.Other;
        affected.CurrentBattle.AdditionalTexts.Add($"{affected} cannot move because they are frozen!");
        return new UsageResult(this)
        {
            Text = "c-cold...",
            UsageType = usageType,
            User = affected,
            TargetType = TargetType.None
        };
    }
}