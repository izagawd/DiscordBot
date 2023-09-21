using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Results;

namespace DiscordBotNet.LegendaryBot.Battle.StatusEffects;

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

    public override UsageResult OverridenUsage(Character affected,ref Character target, ref string decision, UsageType usageType)
    {
        decision = "None";
        affected.CurrentBattle.AdditionalTexts.Add($"{affected} cannot move because they are frozen!");
        return new UsageResult(usageType, TargetType.None,"c-cold...");
    }
}