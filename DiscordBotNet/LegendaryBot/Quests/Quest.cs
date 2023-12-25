using System.Reflection;
using DiscordBotNet.Database.Models;
using DiscordBotNet.LegendaryBot.Rewards;

namespace DiscordBotNet.LegendaryBot.Quests;

public abstract class Quest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public static IEnumerable<Quest> ExampleQuests => _exampleQuests.ToArray();
    private static List<Quest> _exampleQuests;
    static Quest()
    {
        _exampleQuests = new List<Quest>();
        foreach (var  i in Assembly.GetExecutingAssembly().GetTypes().Where(i => i.IsSubclassOf(typeof(Quest)) && !i.IsAbstract))
        {
            _exampleQuests.Add((Quest)Activator.CreateInstance(i)!);
        }
    }
    public Tier QuestTier { get; private set; } = Tier.Bronze;
    /// <returns>True if quest was successfully completed</returns>
    public abstract Task<bool> StartQuest(UserData userDataToStartQuest);

    public abstract IEnumerable<Reward> GetQuestRewards();
    
    public ulong UserDataId { get; set; }
 
}