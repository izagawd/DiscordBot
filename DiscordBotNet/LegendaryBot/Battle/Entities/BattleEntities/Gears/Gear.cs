using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities;
using DiscordBotNet.LegendaryBot.Battle.Results;
using DiscordBotNet.LegendaryBot.Battle.Stats;

namespace DiscordBotNet.LegendaryBot.Battle.Entities.Gears;

public abstract class Gear : BattleEntity
{

    [NotMapped]
    public GearStat MainStat { get; protected set; }
    public AttackFlatGearStat Idk { get; protected set; }
    [NotMapped]
    public GearStat? SubStat1 { get; protected set; }
    [NotMapped]
    public GearStat? SubStat2 { get; protected set; }
    [NotMapped]
    public GearStat? SubStat3 { get; protected set; }
    [NotMapped]
    public GearStat? SubStat4 { get; protected set; }

    

    public GearStat? this[Type type]{
        get
        {
            return AllStats.FirstOrDefault(i => i.GetType() ==  type);
        }
    }



    public override ExperienceGainResult IncreaseExp(ulong experience)
    {
        var nextLevelEXP = BattleFunction.NextLevelFormula(Level);
        string expGainText = "";
        var levelBefore = Level;
        Experience += experience;
        while (Experience >= nextLevelEXP && Level < 15)
        {
            Experience -= nextLevelEXP;
            Level += 1;
            var allStats = GearStat.AllGearStatTypes
                .Where(i => i != GearStat.SpeedPercentageType)
                .ToArray();
            if (Level == 3 || Level == 6 || Level == 9 || Level == 12 || Level == 15)
            {
                if (SubStat1 is null)
                {
                    var randomStat = BasicFunction.RandomChoice(allStats.Except(SubstatTypes));
                    SubStat1 = randomStat;
                    SubStat1.Increase(Rarity);
                }
                else if (SubStat2 is null)
                {
                    var randomStat = BasicFunction.RandomChoice(allStats.Except(SubstatTypes));
                    SubStat2 = randomStat;
                    SubStat2.Increase(Rarity);
                }

                else if (SubStat3 is null)
                {
                    var randomStat = BasicFunction.RandomChoice(allStats.Except(SubstatTypes));
                    SubStat3 = randomStat;
                    SubStat3.Increase(Rarity);
                }

                else if (SubStat4 is null)
                {
                    var randomStat = BasicFunction.RandomChoice(allStats.Except(SubstatTypes));
                    SubStat4 = randomStat;
                    SubStat4.Increase(Rarity);
                }
                else
                {
                    GearStat randomStat = BasicFunction.RandomChoice(Substats);
                    randomStat.Increase(Rarity);
                }
                
               
            }
            
            nextLevelEXP = BattleFunction.NextLevelFormula(Level);
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
 
    public override async Task LoadAsync()
    {
        await base.LoadAsync();
        MainStat.SetMainStat(Rarity,Level);
    }
    /// <summary>
    /// A gear must be initiated before it's first use
    /// </summary>
    /// <param name="rarity">The rarity you want the gear to be</param>
    /// <param name="customMainStat">If the gear should have a main stat instead of being randomized, select it</param>

    public void Initiate(Rarity rarity, Type?  customMainStat = null)
    {
        if (!IsNew) return;

        if (customMainStat is not null && !GearStat.AllGearStatTypes.Contains(customMainStat))
        {
            throw new Exception("Type is not a subclass of GearStat or is abstract");

        }
        if (IsNew && Level == 1)
        {
            Rarity = rarity;
            var statTypeCollection = GearStat.AllGearStatTypes.Where(i => i != GearStat.SpeedPercentageType);
            if (customMainStat is not null)
            {
                if (!PossibleMainStats.Contains(customMainStat))
                {
                    throw new Exception("Gear of this type cannot have a mainstat of "+BasicFunction.Englishify(customMainStat.ToString()!));
                }

                MainStat =(GearStat) Activator.CreateInstance(customMainStat)!;

            }
            else
            {
                var selectedType = BasicFunction.RandomChoice(PossibleMainStats);
                MainStat =(GearStat) Activator.CreateInstance(selectedType)!;
            }
         
            for (int i = 0; i < (int)Rarity; i++)
            {
                var list = SubstatTypes.ToList();
                list.Add(MainStat.GetType());
                var randomStat = BasicFunction.RandomChoice(statTypeCollection.Except(list));
                if (SubStat1 is null)
                {
                    SubStat1 = randomStat;
                }
                else if (SubStat2 is null)
                {
                    SubStat2 = randomStat;
                }
                else if (SubStat3 is null)
                {
                    SubStat3 = randomStat;
                }
                else if (SubStat4 is null)
                {
                    SubStat4 = randomStat;
                }else
                {
                    throw new Exception("All SubStats are already assigned.");
                }

                this[randomStat]!.Increase(Rarity);
            }

            IsNew = false;
        }
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