using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Results;

namespace DiscordBotNet.LegendaryBot.Battle.Moves;

public class SurgeSample : Surge
{
    public override int MaxEnhance { get; } = 4;
    public override string GetDescription(int moveLevel)
    {
        return "idk";
    }

    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return new Character[] { };
    }

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        return new UsageResult(this)
        {
            TargetType = TargetType.None,
            User =owner,
            UsageType = UsageType.NormalUsage
        };
    }

    public override int GetMaxCooldown(int level)
    {
        return 5;
    }
}