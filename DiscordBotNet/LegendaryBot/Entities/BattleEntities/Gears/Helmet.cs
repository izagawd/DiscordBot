using DiscordBotNet.LegendaryBot.Stats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

public class Helmet : Gear
{
    public sealed override IEnumerable<Type> PossibleMainStats { get; } = new[] { GearStat.HealthFlatType };
}