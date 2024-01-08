using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class Poison : StatusEffect, IDetonatable
{
    public override string Description { get; } = "Deals damage equivalent to 5% of the affected's max health";
    public override StatusEffectType EffectType => StatusEffectType.Debuff;
    public override bool HasLevels => false;
    public override bool ExecuteStatusEffectAfterTurn => false;
    public Poison( Character caster) : base(caster)
    {

    }


    public override void PassTurn(Character affected)
    {
        base.PassTurn(affected);

        DoDamage(affected);

    }

    private DamageResult? DoDamage(Character affected)
    {
        return affected.FixedDamage(        new DamageArgs(this)
        {

            Damage = affected.MaxHealth * 0.05,
            Caster = Caster,
            CanCrit = false,
            DamageText =$"{affected} took $ damage from being poisoned!"
        });
    }
    public DamageResult? Detonate(Character affected, Character detonator)
    {
        var removed = affected.StatusEffects.Remove(this);
        if (removed) return DoDamage(affected);
        return null;
    }
}