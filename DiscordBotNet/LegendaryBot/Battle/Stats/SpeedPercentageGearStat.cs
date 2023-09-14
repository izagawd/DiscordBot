﻿using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Gears;

public class SpeedPercentageGearStat : GearStat
{
    public override int GetMainStat(Rarity rarity, int level)
    {
        throw new Exception("Speed percentage should never be a mainstat");
    }

    public override void AddStats(Character character)
    {
        character.TotalSpeed += Value * 0.01 * character.BaseSpeed;
    }

    public override int GetMaximumSubstatLevelIncrease(Rarity rarity)
    {
        throw new Exception("Percentage speed cannot be a main or substat in a gear");
    }

    public override int GetMinimumSubstatLevelIncrease(Rarity rarity)
    {
        throw new Exception("Percentage speed cannot be a main or substat in a gear");
    }
}