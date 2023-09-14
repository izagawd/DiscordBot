using DiscordBotNet.LegendaryBot.Battle.Results;

namespace DiscordBotNet.LegendaryBot.Battle.BattleEvents.EventArgs;

public class CharacterUseSkillEventArgs : System.EventArgs
{
    public UsageResult UsageResult { get; }

    public CharacterUseSkillEventArgs(UsageResult usageResult)
    {
        UsageResult = usageResult;
    }
}