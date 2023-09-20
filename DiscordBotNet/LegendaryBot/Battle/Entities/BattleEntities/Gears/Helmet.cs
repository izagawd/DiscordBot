using DiscordBotNet.LegendaryBot.Battle.Stats;

namespace DiscordBotNet.LegendaryBot.Battle.Entities.Gears;

public class Helmet : Gear
{
    public sealed override IEnumerable<Type> PossibleMainStats { get; } = new[] { GearStat.HealthFlatType };
}