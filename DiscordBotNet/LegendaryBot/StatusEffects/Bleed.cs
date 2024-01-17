using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class Bleed : StatusEffect, IDetonatable
{
    public override string Description => "Does damage proportional to the caster's attack to the affected at the start of the affected's turn." +
                                          " Ignores 70% of the affecteed's defense";
    public override bool ExecuteStatusEffectAfterTurn => false;
    public int Attack { get;  }
    public DamageResult? Detonate(Character affected, Character detonator)
    {
        var removed =affected.RemoveStatusEffect(this);
        if (removed) return DoDamage(affected);
        return null;

    }

    private DamageResult? DoDamage(Character affected)
    {
        return affected.Damage(new DamageArgs(this)
        {
            DefenseToIgnore = 70,
            AffectedByCasterElement = false,
            Damage = Attack,
            DamageText = $"{affected} took $ bleed damage!",
            Caster = Caster,
        });
    }
    public override void PassTurn(Character affected)
    {
        base.PassTurn(affected);
        DoDamage(affected);
    }

    public Bleed(Character caster) : base(caster)
    {
        Attack = caster.Attack;
    }
}