using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.ModifierInterfaces;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class DefenseBuff: StatusEffect
{




    public override int MaxStacks => 1;

    public override StatusEffectType EffectType => StatusEffectType.Buff;

    public DefenseBuff( Character caster) : base(caster)
    {

    }

    public override IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs(Character owner)
    {
        return new StatsModifierArgs[]
        {
            new  DefensePercentageModifierArgs()
            {
                CharacterToAffect = owner,
                ValueToChangeWith = -50,
                WorksAfterGearCalculation = true
            }
        };
    }
}