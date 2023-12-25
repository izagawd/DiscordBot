using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot;

public interface ICanBeLeveledUp
{
    public int Level { get; }
    public ulong Experience { get; }

    public ulong GetRequiredExperienceToNextLevel(int level);
    public ulong GetRequiredExperienceToNextLevel();

    public ExperienceGainResult IncreaseExp(ulong exp);
}