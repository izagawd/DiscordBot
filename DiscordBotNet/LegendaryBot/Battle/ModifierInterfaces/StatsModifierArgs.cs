using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.Battle.ModifierInterfaces;

public abstract class StatsModifierArgs 
{
    /// <summary>
    /// The value to either increase or decrease with
    /// </summary>
    public float ValueToChangeWith { get; init; }
    
    public Character CharacterToAffect { get; init; }
 
    public bool WorksAfterGearCalculation { get; init; } = true;


}