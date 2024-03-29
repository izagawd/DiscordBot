﻿using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.ModifierInterfaces;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class DefenseDebuff: StatusEffect
{




    public override int MaxStacks => 1;

    public override StatusEffectType EffectType => StatusEffectType.Debuff;

    public DefenseDebuff( Character caster) : base( caster)
    {

    }
    public override IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs(Character owner)
    {
        return
        [
            new DefensePercentageModifierArgs
            {
                CharacterToAffect = owner,
                ValueToChangeWith = -50,
                WorksAfterGearCalculation = true
            }
        ];
    }
}