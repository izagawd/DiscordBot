using DiscordBotNet.LegendaryBot.Stats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

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