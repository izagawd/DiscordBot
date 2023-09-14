﻿using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Results;

namespace DiscordBotNet.LegendaryBot.Battle.StatusEffects;

public class Burn : StatusEffect
{
    private int _characterAttack;
    public override StatusEffectType EffectType => StatusEffectType.Debuff;
    public override bool HasLevels => false;
    public override bool ExecuteStatusEffectAfterTurn => false;
    public Burn( Character caster) : base(caster)
    {
        _characterAttack = caster.Attack;
    }


    public override void PassTurn(Character affected)
    {
        base.PassTurn(affected);
        affected.Damage(        new DamageArgs()
        {
            StatusEffect = this,
            Damage = _characterAttack /3.0,
            Caster = Caster,
            CanCrit = false,
            DamageText =$"{affected} took $ damage from burn!"
        });

    }
}