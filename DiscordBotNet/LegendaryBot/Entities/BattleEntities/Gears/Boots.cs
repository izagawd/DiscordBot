using DiscordBotNet.LegendaryBot.Stats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

public class Boots : Gear
{
    public sealed override IEnumerable<Type> PossibleMainStats { get; } =
        [
            GearStat.AttackPercentageType,
            GearStat.SpeedFlatType,
            GearStat.DefensePercentageType,
            GearStat.HealthPercentageType,
            GearStat.DefenseFlatType,
            GearStat.AttackFlatType,
            GearStat.HealthFlatType
        ];
}