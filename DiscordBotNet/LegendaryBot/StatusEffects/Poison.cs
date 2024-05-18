﻿using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class Poison : StatusEffect, IDetonatable
{
    public override string Description => "Deals damage equivalent to 5% of the affected's max health";
    public override StatusEffectType EffectType => StatusEffectType.Debuff;

    public override bool ExecuteStatusEffectAfterTurn => false;
    public Poison( Character caster) : base(caster)
    {

    }


    public override void PassTurn()
    {
        base.PassTurn();

        DoDamage();

    }

    private DamageResult? DoDamage()
    {
        return Affected.FixedDamage(new DamageArgs(this)
        {
            ElementToDamageWith = null,
            Damage = Affected.MaxHealth * 0.05f,
            Caster = Caster,
            CanCrit = false,
            DamageText =$"{Affected} took $ damage from being poisoned!"
        });
    }
    public DamageResult? Detonate( Character detonator)
    {
        var removed = Affected.RemoveStatusEffect(this);
        if (removed) return DoDamage();
        return null;
    }
}