using DiscordBotNet.LegendaryBot.Battle.Stats;

namespace DiscordBotNet.LegendaryBot.Battle.Entities.Gears;

public class Boots : Gear
{
    public sealed override IEnumerable<Type> PossibleMainStats { get; } =
        new[]
        {
            GearStat.AttackPercentageType,
            GearStat.SpeedFlatType,
            GearStat.DefensePercentageType,
            GearStat.HealthPercentageType,
            GearStat.DefenseFlatType,
            GearStat.AttackFlatType,
            GearStat.HealthFlatType
        };
}