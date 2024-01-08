using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Results;

public class DamageArgs
{
    /// <summary>
    /// The percentage of defense to ignore if possible
    /// </summary>
    public int DefenseToIgnore { get; init; } = 0;
    public bool AffectedByCasterElement { get; init; } = true;
    public Move? Move { get; private set; }
    public StatusEffect? StatusEffect { get; private set; }

    public DamageArgs(Move move)
    {
        Move = move;
    }

    public DamageArgs(StatusEffect statusEffect)
    {
        StatusEffect = statusEffect;
    }
    public required double Damage
    {
        get; init;
    }
    /// <summary>
    /// The one who casted the attack
    /// </summary>
        public required Character Caster { get; init; }
        /// <summary>
        /// Use $ in the string and it will be replaced with the damage
        /// </summary>
        public string? DamageText { get; init; } = null;

        public bool CanBeCountered
        {
            get;
            init;
        } = true;

        /// <summary>
        /// if true, the attack always lands a critical hit. Doesnt work in fixed damage
        /// </summary>
        public bool AlwaysCrits { get; init; } = false;
        /// <summary>
        /// attack can always crit. doesnt work in fixed damage
        /// </summary>
        
        public bool CanCrit { get; init; } = true;

}