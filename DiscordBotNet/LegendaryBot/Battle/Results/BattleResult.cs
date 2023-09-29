using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.Battle.Results;

public class BattleResult
{
    public required CharacterTeam Winners { get; init; }

    public required int Turns { get; init; }
    public required Character? TimedOut { get; init; } 
    public  Character? Forfeited { get; init; }
}