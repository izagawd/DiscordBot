using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.Battle.ModifierInterfaces;

public abstract class StatsModifierArgs 
{
    /// <summary>
    /// The value to either increase or decrease with
    /// </summary>
    public float ValueToChangeWith { get; set; }
    
    public Character CharacterToAffect { get; set; }
 
    public bool WorksAfterGearCalculation { get; set; } = true;


}