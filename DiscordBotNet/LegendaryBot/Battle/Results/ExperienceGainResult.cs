﻿namespace DiscordBotNet.LegendaryBot.Battle.Results;

public class ExperienceGainResult
{

    public ulong ExcessExperience { get; init; }
    public string Text { get; init; } = "";
    public override string ToString()
    {
        return Text;
    }
}