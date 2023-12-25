using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.Results;

public class BattleResult
{
    public required ulong Coins { get; init; }
    public required CharacterTeam Winners { get; init; }

    public required int Turns { get; init; }
    public required CharacterTeam? TimedOut { get; init; } 
    public  CharacterTeam? Forfeited { get; init; }
}