﻿using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.Battle.Stats;

public class HealthPercentageGearStat : GearStat
{
    public override int GetMainStat(Rarity rarity, int level)
    {
        return (((int) rarity + 1) * (level / 15.0) * 10).Round() + 10;
    }

    public override void AddStats(Character character)
    {
        character.TotalMaxHealth += Value * 0.01 * character.BaseMaxHealth;
    }

    public override int GetMaximumSubstatLevelIncrease(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.OneStar:
                return 4;
            case Rarity.TwoStar:
                return 5;
            case Rarity.ThreeStar:
                return 6;
            case Rarity.FourStar:
                return 7;
            case Rarity.FiveStar:
                return 8;
            default:
                return 4;
        }
    }

    public override int GetMinimumSubstatLevelIncrease(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.OneStar:
                return 2;
            case Rarity.TwoStar:
            case Rarity.ThreeStar:
            case Rarity.FourStar:
                return 3;
            default:
                return 4;
        }
    }
}