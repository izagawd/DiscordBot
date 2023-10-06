﻿using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.Battle.Stats;

public class AttackFlatGearStat : GearStat
{
    public override int GetMainStat(Rarity rarity, int level)
    {
        return (((int) rarity + 1) * (level / 15.0) * 80).Round() + 100;
    }

    public override void AddStats(Character character)
    {
        character.TotalAttack += Value;
    }

    public override int GetMaximumSubstatLevelIncrease(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.OneStar:
                return 10;
            case Rarity.TwoStar:
                return 20;
            case Rarity.ThreeStar:
                return 30;
            case Rarity.FourStar:
                return 40;
            case Rarity.FiveStar:
                return 50;
            default:
                return 50;
        }
    }

    public override int GetMinimumSubstatLevelIncrease(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.OneStar:
                return 5;
            case Rarity.TwoStar:
                return 10;
            case Rarity.ThreeStar:
                return 15;
            case Rarity.FourStar:
                return 20;
            case Rarity.FiveStar:
                return 25;
            default:
                return 25;
        }
    }
}