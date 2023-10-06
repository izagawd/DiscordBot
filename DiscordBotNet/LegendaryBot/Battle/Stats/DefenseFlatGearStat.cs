﻿using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.Battle.Stats;

public class DefenseFlatGearStat: GearStat
{
    public override int GetMainStat(Rarity rarity, int level)
    {
        return (((int) rarity + 1) * (level / 15.0) * 50).Round() + 50;
    }

    public override void AddStats(Character character)
    {
        character.TotalDefense += Value;
    }

    public override int GetMaximumSubstatLevelIncrease(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.OneStar:
                return 8;
            case Rarity.TwoStar:
                return 18;
            case Rarity.ThreeStar:
                return 26;
            case Rarity.FourStar:
                return 35;
            case Rarity.FiveStar:
                return 45;
            default:
                return 50;
        }
    }

    public override int GetMinimumSubstatLevelIncrease(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.OneStar:
                return 4;
            case Rarity.TwoStar:
                return 9;
            case Rarity.ThreeStar:
                return 13;
            case Rarity.FourStar:
                return 17;
            case Rarity.FiveStar:
                return 22;
            default:
                return 25;
        }
    }
}