using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Moves;
using DiscordBotNet.LegendaryBot.Battle.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Battle.Results;

public class DamageArgs
{
    public Move? Move { get; set; }
    public StatusEffect? StatusEffect { get; set; }
    public double Damage { get; set; }
        public Character Caster { get; set; }
        public string? DamageText { get; set; } = null;

        public bool CanBeCountered
        {
            get;
            set;
        } = true;
            /// <summary>
            /// can crit doesnt work in fixed damage
            /// </summary>
        public bool CanCrit { get; set; } = true;

}