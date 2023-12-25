using DiscordBotNet.LegendaryBot.Stats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

public class Weapon : Gear
{
    public sealed override IEnumerable<Type> PossibleMainStats { get; } = new[] { GearStat.AttackFlatType };
}