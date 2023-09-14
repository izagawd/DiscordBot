using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.ModifierInterfaces;

namespace DiscordBotNet.LegendaryBot.Battle.StatusEffects;

public class AttackDebuff : StatusEffect, IStatsModifier
{

    public override bool HasLevels => true;
    public override int MaxStacks => 1;
    public override int MaxLevel => 3;
    public override StatusEffectType EffectType => StatusEffectType.Debuff;

    public AttackDebuff( Character caster) : base(caster)
    {

    }


    public float AttackPercentage
    {
        get
        {
            switch (Level)
            {
                case 1:
                    return -30;
             
                case 2:
                    return -50;
                default:
                    return -70;
            }
        }
    }

 

    public IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs(Character owner) 
    {
        return new List<StatsModifierArgs>()
        {
            new AttackPercentageModifierArgs()
            {
                CharacterToAffect = owner,
                ValueToChangeWith = AttackPercentage,
                WorksAfterGearCalculation = true
            }
        };
    }


}