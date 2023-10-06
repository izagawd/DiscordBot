﻿using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.Battle.Stats;

public class HealthFlatGearStat : GearStat
{
    public override int GetMainStat(Rarity rarity, int level)
    {

        return (((int)rarity + 1) * (level / 15.0) * 450).Round() + 450;
    }

    public override void AddStats(Character character)
    {
        character.TotalMaxHealth += Value;
    }

    public override int GetMaximumSubstatLevelIncrease(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.OneStar:
                return 40;
            case Rarity.TwoStar:
                return 80;
            case Rarity.ThreeStar:
                return 120;
            case Rarity.FourStar:
                return 160;
            case Rarity.FiveStar:
                return 250;
            default:
                return 250;
        }
    }

    public override int GetMinimumSubstatLevelIncrease(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.OneStar:
                return 20;
            case Rarity.TwoStar:
                return 40;
            case Rarity.ThreeStar:
                return 60;
            case Rarity.FourStar:
                return 80;
            case Rarity.FiveStar:
                return 125;
            default:
                return 125;
        }
    }
}