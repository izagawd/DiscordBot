using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.ModifierInterfaces;

namespace DiscordBotNet.LegendaryBot.Battle.StatusEffects;

public class DefenseBuff: StatusEffect, IStatsModifier
{
    public override bool IsRenewable => true;

    public float DefensePercentage
    {
        get
        {
            switch (Level)
            {
                case 1:
                    return 30;
                case 2:
                    return 50;
                default:
                    return 70;
            }
        }
    }
    public override bool HasLevels => true;
    public override int MaxStacks => 1;
    public override int MaxLevel => 3;
    public override StatusEffectType EffectType => StatusEffectType.Buff;

    public DefenseBuff( Character caster) : base(caster)
    {

    }

    public IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs(Character owner)
    {
        return new List<DefensePercentageModifierArgs>()
        {
            new ()
            {
                CharacterToAffect = owner,
                ValueToChangeWith = DefensePercentage,
                WorksAfterGearCalculation = true
            }
        };
    }
}