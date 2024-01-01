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
        foreach (var i in AllStats)
        {
            i.AddStats(character);
        }
    }
    public GearStat MainStat { get; protected set; }

    public GearStat? SubStat1 { get; protected set; }

    public GearStat? SubStat2 { get; protected set; }

    public GearStat? SubStat3 { get; protected set; }

    public GearStat? SubStat4 { get; protected set; }

    

    public GearStat? this[Type type]{
        get
        {
            return AllStats.FirstOrDefault(i => i.GetType() ==  type);
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
            && !substatsToPrioritize.All(i => GearStat.AllGearStatTypes.Contains(i)))
        {
            throw new Exception("One of the substats to prioritize is not of a substat type or is abstract");
        }

        IEnumerable<Type> availableStats = substatsToPrioritize
            .Except(AllStatTypes).ToArray();
        if (!availableStats.Any())
        {
            availableStats = GearStat
                .AllGearStatTypes
                .Except(AllStatTypes);
        }

        var randomStatType = BasicFunction.RandomChoice(availableStats);
        GearStat randomStat;
        if (SubStat1 is null)
        {
            randomStat = SubStat1 = GearStat.CreateGearStatInstance(randomStatType);
        }
        else if (SubStat2 is null)
        {
            randomStat = SubStat2 =GearStat.CreateGearStatInstance(randomStatType);
        }

        else if (SubStat3 is null)
        {
            randomStat = SubStat3 = GearStat.CreateGearStatInstance(randomStatType);
        }

        else if (SubStat4 is null)
        {
            randomStat = SubStat4 = GearStat.CreateGearStatInstance(randomStatType);
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
    ///<param name="substatsToPrioritize">These stats will take top priority when assigning or increasing substats. if it cannot be assigned or increased, it will pay attention to other substat types</param>

    public ExperienceGainResult IncreaseExp(long experience, params Type[] substatsToPrioritize)
    {
        if (IsNew) throw new Exception("Gear has not been initiated");
        if (substatsToPrioritize.Any() 
            && !substatsToPrioritize.All(i => GearStat.AllGearStatTypes.Contains(i)))
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
        long excessExp = 0;
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
        if (customMainStat is not null && !GearStat.AllGearStatTypes.Contains(customMainStat))
            throw new Exception("CustomMainStatType Type is not a subclass of GearStat or is abstract");
        if (priorityTypes.Any() && !priorityTypes.All(i => GearStat.AllGearStatTypes.Contains(i)))
            throw new Exception("One of the priority types is not a subclass of GearStat or is abstract");

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
            typesToUse = typesToUse.Except(AllStatTypes).ToArray();
            if (!typesToUse.Any())
                typesToUse = GearStat.AllGearStatTypes.ToArray();
            var randomStat = BasicFunction.RandomChoice(typesToUse);
            if (SubStat1 is null)
            {
                SubStat1 =GearStat.CreateGearStatInstance(randomStat);
            }
            else if (SubStat2 is null)
            {
                SubStat2 = GearStat.CreateGearStatInstance(randomStat);
            }
            else if (SubStat3 is null)
            {
                SubStat3 = GearStat.CreateGearStatInstance(randomStat);
            }
            else if (SubStat4 is null)
            {
                SubStat4 = GearStat.CreateGearStatInstance(randomStat);
            }else
            {
                throw new Exception("All SubStats are already assigned.");
            }

            this[randomStat]!.Increase(Rarity);
        }
        IsNew = false;
    }
    
    public sealed override int MaxLevel => 15;
    /// <summary>
    /// The list of all the possible mainstats a gear can have
    /// </summary>
    [NotMapped]
    
    public abstract IEnumerable<Type> PossibleMainStats { get; }
    [NotMapped]
    public IEnumerable<Type> AllStatTypes => AllStats.Select(i => i.GetType());
    [NotMapped]
    public IEnumerable<GearStat> AllStats
    {
        get
        {
            var list = Substats.ToList();
            if (MainStat is not null)
            {
                list.Add(MainStat);
            }
            return list;
        }
    }
    [NotMapped]
    /// <summary>
    /// gets all the substats of the gear
    /// </summary>
    public IEnumerable<Type> SubstatTypes =>
        Substats.Select(i => i.GetType());
    [NotMapped]
    public IEnumerable<GearStat> Substats => new [] { SubStat1, SubStat2, SubStat3, SubStat4 }
        .Where(i => i is not null)
        .OfType<GearStat>();
    public override Rarity Rarity { get; protected set; } = Rarity.OneStar;


}