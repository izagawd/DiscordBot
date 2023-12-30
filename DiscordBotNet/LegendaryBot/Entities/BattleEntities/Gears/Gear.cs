using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.Stats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

public abstract class Gear : BattleEntity
{
    /// <summary>
    /// Adds all the stats from a gear to a character
    /// </summary>
    public void AddStats(Character character)
    {
        foreach (var i in Stats)
        {
            i.AddStats(character);
        }
    }


    
    /// <summary>
    /// The name of the type of main stat
    /// </summary>
    public string MainStatTypeName { get; protected set; } 
    
    
    [NotMapped]
    public GearStat MainStat {
        get
        {
            return Stats.FirstOrDefault(i => i.GetType().Name == MainStatTypeName)!;
        }
        protected set
        {
            Stats.Remove(MainStat);
            MainStatTypeName = value.GetType().Name;
            if(!Stats.Contains(value))
                Stats.Add(value);
        }
    } 

    public List<GearStat> Stats { get; protected set; } = [];
    [NotMapped]
    public IEnumerable<GearStat> Substats
    {
        get
        {
            return Stats.Where(i => i.GetType().Name != MainStatTypeName);
        }
    }


    public GearStat? this[Type type]{
        get
        {
            return Stats.FirstOrDefault(i => i.GetType() ==  type);
        }
    }
    /// <summary>
    /// Upgrades or adds a substat in the gear
    /// </summary>
    /// <param name="substatsToPrioritize">One of these substat types will be added/increment. If it can't it will direct attention to another substat </param>
    public void UpgradeSubStats(params Type[] substatsToPrioritize)
    {
        if (IsNew) throw new Exception("Gear has not been initiated");
        if (substatsToPrioritize.Any() 
            && !substatsToPrioritize.All(i => GearStat.NonAbstractGearStatTypes.Contains(i)))
        {
            throw new Exception("One of the substats to prioritize is not of a gearstat type or is abstract");
        }

        IEnumerable<Type> availableStats = substatsToPrioritize
            .Except(StatTypes)
            .ToArray();
        if (!availableStats.Any())
        {
            availableStats = GearStat
                .NonAbstractGearStatTypes
                .Except(StatTypes);
        }

        var randomStatType = BasicFunction.RandomChoice(availableStats);
        GearStat randomStat;
        
        if (Substats.Count() < 4)
        {
            randomStat = GearStat.CreateGearStatInstance(randomStatType);
    
            Stats.Add(randomStat);
        }

        else
        {
            var statsToUpgrade = Substats
                .Where(i => substatsToPrioritize.Contains(i.GetType()))
                .ToArray();
            if (!statsToUpgrade.Any())
                statsToUpgrade = Substats.ToArray();
            randomStat = BasicFunction.RandomChoice(statsToUpgrade);
        }
        randomStat.Increase(Rarity);
        
    }
    /// <summary>
    /// Checks if the list of stats is valid
    /// </summary>
    public bool VerifyStats()
    {
        return Stats.Count(i => i.GetType().Name == MainStatTypeName) == 1;
    }
    ///<param name="substatsToPrioritize">These stats will take top priority when assigning or increasing substats. if it cannot be assigned or increased, it will pay attention to other substat types</param>

    public ExperienceGainResult IncreaseExp(ulong experience, params Type[] substatsToPrioritize)
    {
        if (IsNew) throw new Exception("Gear has not been initiated");
        if (substatsToPrioritize.Any() 
            && !substatsToPrioritize.All(i => GearStat.NonAbstractGearStatTypes.Contains(i)))
        {
            throw new Exception("One of the substats to prioritize is not of a substat type or is abstract");
        }

        var nextLevelEXP = BattleFunction.NextLevelFormula(Level);
        string expGainText = "";
        var levelBefore = Level;
        Experience += experience;
        while (Experience >= nextLevelEXP && Level < 15)
        {
            Experience -= nextLevelEXP;
            Level += 1;
            if (Level == 3 || Level == 6 || Level == 9 || Level == 12 || Level == 15)
            {
                UpgradeSubStats(substatsToPrioritize);
            }
        }
        expGainText += $"{this} gear gained {experience} exp";
        if (levelBefore != Level)
        {
            expGainText += $", and moved from level {levelBefore} to level {Level}";
        }

        expGainText += "!";
        ulong excessExp = 0;
        if (Experience > nextLevelEXP)
        {
            excessExp = Experience - nextLevelEXP;
        }

        return new ExperienceGainResult(){Text = expGainText, ExcessExperience = excessExp};
    }


    public bool IsNew { get; protected set; } = true;
 
    public sealed override async Task LoadAsync()
    {
        if (IsNew) throw new Exception("Gear has not been initiated");
        await base.LoadAsync();
        MainStat.SetMainStat(Rarity,Level);
    }
    /// <summary>
    /// A gear must be initiated before it's first use
    /// </summary>
    /// <param name="rarity">The rarity you want the gear to be</param>
    /// <param name="customMainStat">If the gear should have a main stat instead of being randomized, select it</param>
    ///<param name="priorityTypes">These stats will take top priority when assigning substats yo this gear</param>
    public void Initiate(Rarity rarity, Type?  customMainStat = null, params Type[] priorityTypes)
    {
        if (!IsNew) throw new Exception("Gear has already been initiated");
        if (customMainStat is not null && !GearStat.NonAbstractGearStatTypes.Contains(customMainStat))
            throw new Exception("CustomMainStatType Type is not a subclass of GearStat or is abstract");
        if (priorityTypes.Any() && !priorityTypes.All(i => GearStat.NonAbstractGearStatTypes.Contains(i)))
            throw new Exception("At least one of the priority types is not a subclass of GearStat or is abstract");

        var typesToUse = priorityTypes.ToArray();
        Rarity = rarity;
        if(Level != 1)return;
        if (customMainStat is not null && !PossibleMainStats.Contains(customMainStat))
            throw new Exception("Custom main stat type cannot be assigned to this type of gear");

        Type selectedType = customMainStat;
        if (selectedType is null)
        {
            selectedType = BasicFunction.RandomChoice(PossibleMainStats);
        }
        
        MainStat =(GearStat) Activator.CreateInstance(selectedType)!;
        
        for (int i = 0; i < (int)Rarity; i++)
        {
            typesToUse = typesToUse.Except(StatTypes).ToArray();
            if (!typesToUse.Any())
                typesToUse = GearStat.NonAbstractGearStatTypes.ToArray();
            var randomStatType = BasicFunction.RandomChoice(typesToUse);

            if (Substats.Count() < 4)
            {
                var substatToAdd = GearStat.CreateGearStatInstance(randomStatType);
                Stats.Add(substatToAdd);
            }
            else
            {
                throw new Exception("All Substats are already assigned.");
            }

            this[randomStatType]!.Increase(Rarity);
        }
        IsNew = false;
    }
    
    public sealed override int MaxLevel => 15;
    /// <summary>
    /// The list of all the possible mainstats a gear can have
    /// </summary>
    [NotMapped]
    
    public abstract IEnumerable<Type> PossibleMainStats { get; }
    /// <summary>
    /// Gets all the stat types of gears the character has
    /// </summary>
    [NotMapped]
    
    public IEnumerable<Type> StatTypes => Stats.Select(i => i.GetType());

    /// <summary>
    /// gets all the substats of the gear
    /// </summary>
    [NotMapped]

    public IEnumerable<Type> SubstatTypes =>
        Substats.Select(i => i.GetType());

    public override Rarity Rarity { get; protected set; } = Rarity.OneStar;


}