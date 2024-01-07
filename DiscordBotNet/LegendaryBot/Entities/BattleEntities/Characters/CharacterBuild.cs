using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using DiscordBotNet.Database.Models;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;



public enum StatType
{
    Attack, MaxHealth,Speed,Effectiveness,Resistance,CriticalDamage,CriticalChance, Defense
}
public class CharacterBuild : IDatabaseModel
{
    public override string ToString()
    {
        var stringBuilder = new StringBuilder("");
        foreach (var i in Enum.GetValues<StatType>())
        {
            stringBuilder.Append($"{BasicFunction.Englishify(i.ToString())} Points: {this[i]}\n");
        }

        stringBuilder.Append($"\nTotal Points: {TotalPoints}");
        stringBuilder.Append($"\nPoints available: {MaxPoints - TotalPoints}");
        return stringBuilder.ToString();
    }

    public long Id { get; set; }
    public long CharacterId { get; set; }
    
    /// <summary>
    /// The character that has this build equipped
    /// </summary>
    public long? EquippedCharacterId { get; set; }
    public Character Character { get; set; }
    public int this[StatType statType]
    {
        get
        {
            switch (statType)
            {
                case StatType.Attack:
                    return AttackPoints;
                case StatType.Effectiveness:
                    return EffectivenessPoints;
                case StatType.Resistance:
                    return ResistancePoints;
                case StatType.Speed:
                    return SpeedPoints;
                case StatType.CriticalChance:
                    return CriticalChancePoints;
                case StatType.CriticalDamage:
                    return CriticalDamagePoints;
                case StatType.MaxHealth:
                    return MaxHealthPoints;
                case StatType.Defense:
                    return DefensePoints;
                default:
                    throw new ArgumentOutOfRangeException(nameof(statType), statType, "Invalid StatType");
            }
        }
        set
        {
            switch (statType)
            {
                case StatType.Attack:
                    AttackPoints = value;
                    break;
                case StatType.Effectiveness:
                    EffectivenessPoints = value;
                    break;
                case StatType.Resistance:
                    ResistancePoints = value;
                    break;
                case StatType.Speed:
                    SpeedPoints = value;
                    break;
                case StatType.CriticalChance:
                    CriticalChancePoints = value;
                    break;
                case StatType.CriticalDamage:
                    CriticalDamagePoints = value;
                    break;
                case StatType.MaxHealth:
                    MaxHealthPoints = value;
                    break;
                case StatType.Defense:
                    DefensePoints = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(statType), statType, "Invalid StatType");
            }
        }
    }
    public string BuildName { get; set; } = "Build 1";
    public void ResetPoints()
    {
        foreach (var i in Enum.GetValues<StatType>())
        {
            this[i] = 0;
        }
    }
    public int TotalPoints =>  Enum.GetValues<StatType>().Sum(i => this[i]);

    public static int MaxPointsPerStat => 67;
    [NotMapped]
    public int MaxPoints
    {
        get
        {
            if (Character is null)
                throw new Exception("character cannot be null when using this property. load it from the db");

            return (Character.Level - 1) * 2;
        }
    }

    public void IncreasePoint(StatType statType)
    {
        if (Character is null)
            throw new NullReferenceException(
                $"{nameof(Character)} property should not be null to use this method. Don't forget to query from db");
        var totalPoints = TotalPoints;
        var maxPoints = MaxPoints;
        if (totalPoints >= maxPoints)
            throw new Exception($"Cannot go beyond {maxPoints}. Either level up or try a different build");
        if (this[statType] >= MaxPointsPerStat)
            throw new Exception($"Reached max amount of points for substat {statType}");
        this[statType]++;
    }
   
    public int AttackPoints { get; protected set; }
    public int MaxHealthPoints { get; protected set; }
    public int SpeedPoints { get; protected set; }
    public int EffectivenessPoints { get; protected set; }
    public int ResistancePoints { get; protected set; }
    public int CriticalDamagePoints { get; protected set; }
    public int CriticalChancePoints { get; protected set; }
    public int DefensePoints { get; protected set; }
}