using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Results;
using SixLabors.ImageSharp.PixelFormats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;

public abstract class Blessing : BattleEntity
{
    private static List<Blessing> _blessingExamples = [];
    private static List<Blessing> _oneStarBlessingExamples = [];
    private static List<Blessing> _twoStarBlessingExamples = [];

    private static List<Blessing> _threeStarBlessingExamples = [];
    private static List<Blessing> _fourStarBlessingExamples = [];
    private static List<Blessing> _fiveStarBlessingExamples = [];

    public static Blessing[] BlessingExamples => _blessingExamples.ToArray();
    public static Blessing[] OneStarBlessingExamples => _oneStarBlessingExamples.ToArray();
    public static Blessing[] TwoStarBlessingExamples => _twoStarBlessingExamples.ToArray();
    public static Blessing[] ThreeStarBlessingExamples => _threeStarBlessingExamples.ToArray();
    public static Blessing[] FourStarBlessingExamples => _fourStarBlessingExamples.ToArray();
    public static Blessing[] FiveStarBlessingExamples => _fiveStarBlessingExamples.ToArray();
    
    private static Type[] _blessingTypes = Assembly.GetExecutingAssembly().GetTypes()
        .Where(i =>i.IsSubclassOf(typeof(Blessing)) && !i.IsAbstract)
        .ToArray();

    public static Type[] BlessingTypes => _blessingTypes.ToArray();
    static Blessing()
    {
        
        foreach (var i in _blessingTypes)
        {
            var instance = Activator.CreateInstance(i);

            if (instance is Blessing blessingInstance)
            {
         
                _blessingExamples.Add(blessingInstance);
            }
        }

        foreach (var i in _blessingExamples)
        {
            switch (i.Rarity)
            {
                case Rarity.OneStar:
                    _oneStarBlessingExamples.Add(i);
                    break;
                case Rarity.TwoStar:
                    _twoStarBlessingExamples.Add(i);
                    break;
                case Rarity.ThreeStar:
                    _threeStarBlessingExamples.Add(i);
                    break;
                case Rarity.FourStar:
                    _fourStarBlessingExamples.Add(i);
                    break;
                case Rarity.FiveStar:
                    _fiveStarBlessingExamples.Add(i);
                    break;
            }
        }

    
    }
    /// <summary>
    /// The description of the blessing in relation to the level provided
    /// </summary>

    public abstract string GetDescription(int level);

    public string Description => GetDescription(Level);
    public override string IconUrl => $"{Website.DomainName}/battle_images/blessings/{GetType().Name}.png";

    public sealed  override int MaxLevel => 15;
    [NotMapped] public virtual int Attack { get; } = 200;
    [NotMapped] public virtual int Health{ get; } = 200;


    public override async Task<Image<Rgba32>> GetDetailsImageAsync()
    {
        var blessingImageSize = 500;
        var blessingImage = await BasicFunction.GetImageFromUrlAsync(IconUrl);
        blessingImage.Mutate(i => i.Resize(blessingImageSize,blessingImageSize));
        return blessingImage;


    }

    public virtual bool IsLimited => false;
    public Character? Character { get; set; }
    public override ExperienceGainResult IncreaseExp(long experience)
    {
        string expGainText = "";

        var levelBefore = Level;
        Experience += experience;
        var nextLevelEXP = BattleFunction.NextLevelFormula(Level);
        while (Experience >= nextLevelEXP &&  Level < MaxLevel)
        {
            Experience -= nextLevelEXP;
            Level += 1;
            nextLevelEXP = BattleFunction.NextLevelFormula(Level);
        }
        long excessExp = 0;
        if (Experience > nextLevelEXP)
        {
            excessExp = Experience - nextLevelEXP;
        }

        expGainText += $"{this} gained {experience} exp";
        if (levelBefore != Level)
        {
            expGainText += $", and moved from level {levelBefore} to level {Level}";
        }

        expGainText += "!";
        return new ExperienceGainResult() { Text = expGainText, ExcessExperience = excessExp };

    }
}