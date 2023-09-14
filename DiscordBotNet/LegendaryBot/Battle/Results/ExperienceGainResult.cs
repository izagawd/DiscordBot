namespace DiscordBotNet.LegendaryBot.Battle.Results;

public class ExperienceGainResult
{
    public ExperienceGainResult()
    {
        ExcessExperience = 0;
    }

    public ulong ExcessExperience { get; set; }
    public string Text { get; set; } = "";
    public override string ToString()
    {
        return Text;
    }
}