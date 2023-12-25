using DiscordBotNet.LegendaryBot.Stats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

public class Armor : Gear
{ 
    public sealed override IEnumerable<Type> PossibleMainStats { get; } = new[] { GearStat.DefenseFlatType };
}