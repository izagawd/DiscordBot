using DiscordBotNet.LegendaryBot.Stats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

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