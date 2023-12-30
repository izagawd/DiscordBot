using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text.Json.Nodes;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DiscordBotNet.LegendaryBot.Stats;


public abstract class GearStat
{
    [NotMapped]
    public static Type AttackPercentageType { get; } = typeof(AttackPercentageGearStat);
    [NotMapped]  public static Type AttackFlatType { get; } = typeof(AttackFlatGearStat);
    [NotMapped] public static Type HealthPercentageType { get; } = typeof(HealthPercentageGearStat);
    [NotMapped]public static Type HealthFlatType { get; } = typeof(HealthFlatGearStat);
    [NotMapped] public static Type DefenseFlatType { get; } = typeof(DefenseFlatGearStat);
    [NotMapped]  public static Type DefensePercentageType { get; } = typeof(DefensePercentageGearStat);
    [NotMapped] public static Type CriticalChanceType { get; } = typeof(CriticalChanceGearStat);
    [NotMapped]  public static Type CriticalDamageType { get; } = typeof(CriticalDamageGearStat);
    [NotMapped]  public static Type ResistanceType { get; } = typeof(ResistanceGearStat);
    [NotMapped]  public static Type EffectivenessType { get; } = typeof(EffectivenessGearStat);
    [NotMapped] public static Type SpeedFlatType { get; } = typeof(SpeedFlatGearStat);

    public Guid Id { get; protected set; } = Guid.NewGuid();

    
    public Gear Gear { get; protected set; }
    public Guid GearId { get; protected set; }
    /// <summary>
    /// This is called when the gear that owns this stat is loaded. It sets the main stat's value according to
    /// the rarity and the level of the gear
    /// </summary>
    /// <param name="rarity"></param>
    /// <param name="level"></param>
    public  void SetMainStat(Rarity rarity, int level)
    {
        Value = GetMainStat(rarity, level);
    }
    

    
    public abstract int GetMainStat(Rarity rarity, int level);
/// <summary>
/// the amount of times a substat has been increased, if this GearStat is a substat
/// </summary>
    public int TimesIncreased { get; private set; }
    /// <summary>
    /// the value of the stat
    /// </summary>
    public int Value { get; protected set; }
    /// <summary>
    /// Adds this stat to a character
    /// </summary>
    public abstract void AddStats(Character character);
    /// <summary>
    /// Creates a gear stat instance from a gear stat type
    /// </summary>
    /// <exception cref="Exception">Throws exception if provided an abstract or a non gear stat class</exception>
    public static GearStat CreateGearStatInstance(Type gearStatType)
    {
        if(gearStatType.IsAbstract)
        {
            throw new Exception($"Cannot create a gear stat instance from an abstract type");
        }
        if (!gearStatType.IsRelatedToType(typeof(GearStat)))
        {
            throw new Exception($"Cannot create gearstat instance from non gear stat type {gearStatType}");
        }
        return (GearStat)Activator.CreateInstance(gearStatType)!;
    }

    /// <summary>
    /// Holds all the gear stat types that are not abstract
    /// </summary>
    [NotMapped]
    
    public static IEnumerable<Type> NonAbstractGearStatTypes { get; }

    [NotMapped]
    
    public static IEnumerable<Type> GearStatTypes { get; }
    static GearStat()
    {

        var allTypes = Assembly.GetExecutingAssembly().GetTypes();
        GearStatTypes = allTypes.Where(i => i.IsRelatedToType(typeof(GearStat)));
        NonAbstractGearStatTypes = allTypes
            .Where(i => !i.IsAbstract && i.IsRelatedToType(typeof(GearStat)))
            .ToImmutableArray();
    }
    
    public override string ToString()
    {
        return $"{BasicFunction.Englishify(GetType().Name.Replace("GearStat",""))}: {Value}";
    }
    /// <summary>
    /// Increases the value of the substat by a random amount between GetMaximumSubstatLevelIncrease
    /// and GetMinimum
    /// </summary>
    /// <param name="rarity"></param>
    public void Increase(Rarity rarity)
    {
        TimesIncreased += 1;
        Value += BasicFunction.GetRandomNumberInBetween(GetMinimumSubstatLevelIncrease(rarity),
            GetMaximumSubstatLevelIncrease(rarity));
    }
    /// <summary>
    /// Gets the maximum amount this stat can increase depending on the rarity of the gear
    /// </summary>
    /// <param name="rarity"></param>
    /// <returns></returns>
    public abstract int GetMaximumSubstatLevelIncrease(Rarity rarity);
    /// <summary>
    /// Gets the minimum amount this stat can increase depending on the rarity of the gear
    /// </summary>
    /// <param name="rarity"></param>
    /// <returns></returns>
    public abstract int GetMinimumSubstatLevelIncrease(Rarity rarity);
}