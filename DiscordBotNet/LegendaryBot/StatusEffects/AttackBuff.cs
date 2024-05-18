﻿using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.ModifierInterfaces;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class AttackBuff : StatusEffect
{


    public override string Description => "Increases the caster's attack by 50%";


    public override int MaxStacks => 1;

    public override StatusEffectType EffectType => StatusEffectType.Buff;

    public AttackBuff(Character caster) : base(caster)
    {

    }




    public override IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs()
    {
        yield return new AttackPercentageModifierArgs
            {
                CharacterToAffect = Affected,
                ValueToChangeWith = 50,
                WorksAfterGearCalculation = true
            };
    }
}