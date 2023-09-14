using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Gears;

public class SpeedFlatGearStat : GearStat
{
    public override int GetMainStat(Rarity rarity, int level)
    {
        return (((int) rarity + 1) * (level / 15.0) * 7).Round() + 10;
    }

    public override void AddStats(Character character)
    {
        character.TotalSpeed += Value;
    }

    public override int GetMaximumSubstatLevelIncrease(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.OneStar:
                return 1;
            case Rarity.TwoStar:
                return 2;
            case Rarity.ThreeStar:
                return 3;
            default:
                return 4;

        }
    }

    public override int GetMinimumSubstatLevelIncrease(Rarity rarity)
    {
        return 1;
    }
}