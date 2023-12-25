using DiscordBotNet.Database.Models;
using DiscordBotNet.LegendaryBot.Rewards;

namespace DiscordBotNet.LegendaryBot.Quests;

public abstract class Quest : Model
{
    public Tier QuestTier { get; private set; } = Tier.Bronze;
    /// <returns>True if quest was successfully completed</returns>
    public abstract Task<bool> StartQuest(UserData userDataToStartQuest);

    public abstract IEnumerable<Reward> GetQuestRewards();
    
    public ulong UserDataId { get; set; }
}