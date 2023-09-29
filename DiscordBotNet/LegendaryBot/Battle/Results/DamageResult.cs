using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Moves;
using DiscordBotNet.LegendaryBot.Battle.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Battle.Results;

public class DamageResult
{
    public  Move? Move { get; private set; }
    public StatusEffect? StatusEffect { get; private set; }

    public DamageResult(Move move)
    {
        Move = move;
    }

    public DamageResult(StatusEffect statusEffect)
    {
        StatusEffect = statusEffect;
    }
    public int Damage { get; init; }
    public bool WasCrit { get; init; }
    public bool CanBeCountered { get; init; } 
    public  Character? DamageDealer { get; init; }
    public required Character DamageReceiver { get; init; }

}