using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities;

public abstract class BattleEntity : Entity, ICanBeLeveledUp
{
    
    public virtual int Level { get; protected set; } = 1;
    [NotMapped]
    public virtual int MaxLevel { get; }
    public virtual ExperienceGainResult  IncreaseExp(ulong experience)
    {
        return new ExperienceGainResult();
    }

    public virtual ulong GetRequiredExperienceToNextLevel(int level)
    {
        return BattleFunction.NextLevelFormula(level);
    }
    public ulong GetRequiredExperienceToNextLevel()
    {
        return GetRequiredExperienceToNextLevel(Level);
    }
    public ulong Experience
    {
        get;
        protected set;
    }

    [NotMapped] public virtual Rarity Rarity { get; protected set; } = Rarity.OneStar;
}