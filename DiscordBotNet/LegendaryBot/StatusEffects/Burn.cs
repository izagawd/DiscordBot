using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class Burn : StatusEffect, IDetonatable
{
    public override string Description => "Does damage at the start of the affected's turn. Damage ignores 70% of defense";
    private float _characterAttack;
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
        if (affected.IsDead) return null;
        return affected.Damage(new DamageArgs(this)
        {
            DefenseToIgnore = 70,
            ElementToDamageWith = null,
            Damage = _characterAttack * 0.6f,
            Caster = Caster,
            CanCrit = false,
            DamageText =$"{affected} took $ damage from burn!"
        });
        
    }

    public DamageResult? Detonate(Character affected, Character detonator)
    { 
        affected.RemoveStatusEffect(this);
        return DoDamage(affected);
    }
}