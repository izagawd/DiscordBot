using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.ModifierInterfaces;

public interface IStatsModifier
{
    /// <returns></returns>
    public IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs();
}