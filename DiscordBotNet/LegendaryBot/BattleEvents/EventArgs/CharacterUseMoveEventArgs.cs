using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;

public class CharacterUseMoveEventArgs : BattleEventArgs
{
    public UsageResult UsageResult { get; }

    public CharacterUseMoveEventArgs(UsageResult usageResult)
    {
        UsageResult = usageResult;
    }
}