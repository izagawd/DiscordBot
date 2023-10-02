using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Results;

namespace DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Blessings;

public abstract class Blessing : BattleEntity
{
    /// <summary>
    /// The description of the blessing in relation to the level provided
    /// </summary>

    public virtual string GetDescription(int level)
    {
        return "";
        
    }

    public string Description => GetDescription(Level);
    public override string IconUrl => $"{Website.DomainName}/battle_images/blessings/{GetType().Name}.png";

    public sealed  override int MaxLevel => 15;
    [NotMapped] public virtual int Attack { get; } = 200;
    [NotMapped] public virtual int Defense { get; } = 200;

    public Character? Character { get; set; }
    public override ExperienceGainResult IncreaseExp(ulong experience)
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
        ulong excessExp = 0;
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