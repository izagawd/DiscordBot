using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Results;

namespace DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Blessings;

public abstract class Blessing : BattleEntity
{
    public override string IconUrl => $"https://legendarygawds.com/blessing-pictures/{GetType().Name}.png";

    public override int MaxLevel => 60;
    [NotMapped]
    public virtual int Attack { get; }
    [NotMapped]
    public virtual int Defense { get; }

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