﻿using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.ModifierInterfaces;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class DefenseBuff: StatusEffect, IStatsModifier
{




    public override int MaxStacks => 1;

    public override StatusEffectType EffectType => StatusEffectType.Buff;

    public DefenseBuff( Character caster) : base(caster)
    {

    }

    public IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs()
    {
        yield return
            new DefensePercentageModifierArgs()
            {
                CharacterToAffect = Affected,
                ValueToChangeWith = -50,
                WorksAfterGearCalculation = true
            };
    }
}