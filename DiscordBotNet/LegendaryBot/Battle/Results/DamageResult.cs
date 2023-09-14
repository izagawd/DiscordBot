using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Moves;
using DiscordBotNet.LegendaryBot.Battle.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Battle.Results;

public class DamageResult
{
    public Move? Move { get; set; }
    public StatusEffect? StatusEffect { get; set; }

    public int Damage { get; set; }
    public bool WasCrit { get; set; }
    public bool CanBeCountered { get; set; } 
    public Character? DamageDealer { get; set; }
    public Character DamageReceiver { get; set; }

}