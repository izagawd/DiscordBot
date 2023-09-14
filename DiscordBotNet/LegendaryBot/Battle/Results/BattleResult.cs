using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.Battle.Results;

public class BattleResult
{
    public CharacterTeam Winners { get; set; }

    public int Turns { get; set; }
    public Character? TimedOut { get; set; } 
    public Character? Forfeited { get; set; }
}