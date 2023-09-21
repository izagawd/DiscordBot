﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Nodes;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Entities.Gears;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DiscordBotNet.LegendaryBot.Battle.Stats;


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
    [NotMapped]  public static Type SpeedPercentageType { get; } = typeof(SpeedPercentageGearStat);

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
    public class CustomTypeValueConverter<TCustomType, TOtherType> : ValueConverter<TCustomType, TOtherType>
    {
        public CustomTypeValueConverter(Func<TCustomType, TOtherType> convertToProvider, Func<TOtherType, TCustomType> convertFromProvider)
            : base(
                v => convertToProvider(v),
                v => convertFromProvider(v))
        {
        }
    }
    public static ValueConverter<GearStat, string> ValueConverter { get;} = 
    
        new CustomTypeValueConverter<GearStat,string>(i =>
                new JsonObject
                {
                    { "TimesIncreased", i.TimesIncreased }, { "Value", i.Value },
                    { "Discriminator", i.GetType().Name }
                }.ToJsonString(),
            i =>
            {
                var doc = JsonNode.Parse(i).AsObject();

                var type = Bot.AllAssemblyTypes.First(i =>
                    i.IsRelatedToType(typeof(GearStat)) && i.Name == doc["Discriminator"]!.GetValue<string>());
                var stat = (GearStat)Activator.CreateInstance(type)!;
                stat.TimesIncreased = doc["TimesIncreased"]!.GetValue<int>();
                stat.Value = doc["Value"]!.GetValue<int>();
                return stat;
            });



    
    public abstract int GetMainStat(Rarity rarity, int level);
/// <summary>
/// the amount of times a substat has been increased, if this GearStat is a substat
/// </summary>
    public int TimesIncreased { get; private set; }
    /// <summary>
    /// the value of the stat
    /// </summary>
    public int Value { get; protected set; }
    
    public abstract void AddStats(Character character);

    public static implicit operator GearStat(Type type)
    {
        if(!type.IsSubclassOf(typeof(GearStat)) || type.IsAbstract)
        {
            return null;
        }

        return (GearStat)Activator.CreateInstance(type)!;
    }
    [NotMapped]
    public static IEnumerable<Type> AllGearStatTypes { get; }

    static GearStat()
    {

        AllGearStatTypes = Bot.AllAssemblyTypes.Where(i => !i.IsAbstract && i.IsSubclassOf(typeof(GearStat)));
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