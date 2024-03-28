using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class Burn : StatusEffect, IDetonatable
{
    public override string Description => "Does damage at the start of the affected's turn. Damage ignores 70% of defense";
    private int _characterAttack;
    public override StatusEffectType EffectType => StatusEffectType.Debuff;

    public override bool ExecuteStatusEffectAfterTurn => false;
    public Burn( Character caster) : base(caster)
    {
        _characterAttack = caster.Attack;
    }


    public override void PassTurn(Character affected)
    {
        base.PassTurn(affected);

        DoDamage(affected);
    }

    private DamageResult? DoDamage(Character affected)
    {
        return affected.Damage(new DamageArgs(this)
        {
            DefenseToIgnore = 70,
            AffectedByCasterElement = false,
            Damage = _characterAttack * 0.6,
            Caster = Caster,
            CanCrit = false,
            DamageText =$"{affected} took $ damage from burn!"
        });
        
    }

    public DamageResult? Detonate(Character affected, Character detonator)
    {
        var removed = affected.RemoveStatusEffect(this);
        if (removed) return DoDamage(affected);
        return null;
    }
}