using DiscordBotNet.LegendaryBot.Battle.Stats;

namespace DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Gears;

public class Ring :  Gear
{
    public sealed override IEnumerable<Type> PossibleMainStats { get; }= 
        new[]
        {
            GearStat.AttackPercentageType,
            GearStat.ResistanceType,
            GearStat.EffectivenessType,
            GearStat.DefensePercentageType,
            GearStat.HealthPercentageType
        };
}