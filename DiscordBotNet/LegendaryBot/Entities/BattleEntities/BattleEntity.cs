﻿using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.LegendaryBot.BattleEvents;
using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.ModifierInterfaces;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities;

public abstract class BattleEntity : Entity, ICanBeLeveledUp, IBattleEventListener, IStatsModifier
{
    
    public virtual int Level { get; protected set; } = 1;
    [NotMapped]
    public virtual int MaxLevel { get; }
    public virtual ExperienceGainResult  IncreaseExp(long experience)
    {
        return new ExperienceGainResult();
    }

    public virtual long GetRequiredExperienceToNextLevel(int level)
    {
        return BattleFunctionality.NextLevelFormula(level);
    }
    public long GetRequiredExperienceToNextLevel()
    {
        return GetRequiredExperienceToNextLevel(Level);
    }
    public long Experience
    {
        get;
        protected set;
    }

    public virtual void OnBattleEvent(BattleEventArgs eventArgs, Character owner)
    {
       
    }

    public virtual IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs(Character owner)
    {
        return [];
    }
}