using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.Battle.Moves;

public abstract class Skill : Special
{
    public sealed override MoveType MoveType => MoveType.Skill;


}