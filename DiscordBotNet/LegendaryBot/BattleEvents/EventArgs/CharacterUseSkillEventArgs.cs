﻿using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;

public class CharacterUseSkillEventArgs : System.EventArgs
{
    public UsageResult UsageResult { get; }

    public CharacterUseSkillEventArgs(UsageResult usageResult)
    {
        UsageResult = usageResult;
    }
}