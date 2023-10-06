using DiscordBotNet.LegendaryBot.Battle.Stats;

namespace DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Gears;

public class Necklace : Gear
{
    public sealed override IEnumerable<Type> PossibleMainStats { get; } = 
        new[]
        {
            GearStat.AttackPercentageType,
            GearStat.CriticalChanceType,
            GearStat.CriticalDamageType,
            GearStat.DefensePercentageType,
            GearStat.HealthPercentageType
        };
}