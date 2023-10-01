using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.Battle.ModifierInterfaces;

public interface IStatsModifier
{
    /// <param name="owner">The person who possesses the entity</param>
    /// <returns></returns>
    public IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs(Character owner);
}