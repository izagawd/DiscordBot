﻿using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Results;

namespace DiscordBotNet.LegendaryBot.Battle.StatusEffects;

public class Bomb : StatusEffect
{
    public override StatusEffectType EffectType => StatusEffectType.Debuff;
    public int Attack { get; }
    public Bomb(Character caster) : base(caster)
    {
        Attack = caster.Attack;
    }

    public override void PassTurn(Character affected)
    {
        base.PassTurn(affected);
        if (Duration == 0)
        {
            Detonate(affected);
        }

        
    }

    public DamageResult Detonate(Character affected)
    {

        affected.StatusEffects.Remove(this);
        return affected.Damage(        new DamageArgs()
        {
            StatusEffect = this,
            Damage = Attack * 1.871,
            Caster = Caster,
            CanCrit = false,
            DamageText = $"Bomb detonated on {affected} and dealt $ damage!"
        });
   
    }
    public override bool ExecuteStatusEffectAfterTurn => false;
}