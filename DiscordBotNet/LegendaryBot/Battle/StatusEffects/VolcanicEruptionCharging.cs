using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Results;

namespace DiscordBotNet.LegendaryBot.Battle.StatusEffects;

public class VolcanicEruptionCharging : StatusEffect
{
    public override bool HasLevels => false;

    public VolcanicEruptionCharging( Character caster) : base(caster)
    {
     
    }
    public override StatusEffectType EffectType => StatusEffectType.Buff;
    public override OverrideTurnType OverrideTurnType => OverrideTurnType.ControlDecision;
    public override int MaxStacks => 1;

    public override UsageResult OverridenUsage(Character affected,ref Character target, ref string decision, UsageType usageType)
    {
        decision = "None";

        if(Duration == 1)
        {
            List<DamageResult> damageResults = new List<DamageResult>();
            foreach (var i in affected.CurrentBattle.Characters.Where(j => j.Team != affected.Team))
            {

                DamageResult damageResult =  i.Damage(                new DamageArgs()
                {
                    StatusEffect = this,
                    Damage = affected.Attack * 4,
                    Caster = affected,
                    CanCrit = true,
                    DamageText =$"{affected} shot out a very powerful blast that dealt $ damage to {i}!"
                });
                damageResults.Add(damageResult);
                return new UsageResult(UsageType.NormalUsage,TargetType.AOE)
                    { Text = "Blast!", DamageResults = damageResults};
            }

        }
        affected.CurrentBattle.AdditionalTexts.Add($"{affected} is charging up a very powerful attack. {Duration-1} more turns till it is released");
        return new UsageResult(usageType,TargetType.None,"Charging!");
    }
    public override void PassTurn(Character affected)
    {
        if (affected.StatusEffects.Any(i => (int)i.OverrideTurnType > (int)OverrideTurnType))
        {
            affected.StatusEffects.Remove(this);
            return;
        }

        base.PassTurn(affected);

    }

}