﻿using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
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


    public override void PassTurn()
    {
        base.PassTurn();
        
        DoDamage();
    }

    private DamageResult? DoDamage()
    {
        if (Affected.IsDead) return null;
        return Affected.Damage(new DamageArgs(this)
        {
            DefenseToIgnore = 70,
            ElementToDamageWith = null,
            Damage = _characterAttack * 0.6f,
            Caster = Caster,
            CanCrit = false,
            DamageText = $"{Affected} took $ damage from burn!"
        });
        
    }

    public DamageResult? Detonate( Character detonator)
    { 
        Affected.RemoveStatusEffect(this);
        return DoDamage();
    }
}