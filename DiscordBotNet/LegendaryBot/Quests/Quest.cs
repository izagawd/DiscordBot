using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using DiscordBotNet.Database.Models;
using DiscordBotNet.LegendaryBot.Rewards;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotNet.LegendaryBot.Quests;

public abstract class Quest
{
    [NotMapped]
    public abstract string Description { get; }
    [NotMapped]
    public virtual string Title => BasicFunction.Englishify(GetType().Name);
    
    public bool Completed { get; set; } = false;
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
    ///<param name="messageToEdit"> if not null, should edit that message instead of adding a new message to the channel</param>
    /// <returns>True if quest was successfully completed</returns>
    public abstract Task<bool> StartQuest(InteractionContext context,DiscordMessage? messageToEdit = null);

    public abstract IEnumerable<Reward> GetQuestRewards();
    
    public ulong UserDataId { get; set; }
 
}