using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using DiscordBotNet.LegendaryBot.Rewards;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotNet.LegendaryBot.Quests;

public abstract class Quest
{
    private static List<Type> _questTypes = [];
    public static IEnumerable<Type> QuestTypes => _questTypes.ToArray();
    [NotMapped]
    public abstract string Description { get; }
    [NotMapped]
    public virtual string Title => BasicFunction.Englishify(GetType().Name);
    
    public bool Completed { get; set; } = false;
    public Guid Id { get; set; } = Guid.NewGuid();
    public static IEnumerable<Quest> QuestSampleInstances => _exampleQuests.ToArray();
    private static List<Quest> _exampleQuests;
    static Quest()
    {

        _exampleQuests = new List<Quest>();
        foreach (var  i in Assembly.GetExecutingAssembly().GetTypes().Where(i => i.IsSubclassOf(typeof(Quest)) && !i.IsAbstract))
        {
            _exampleQuests.Add((Quest)Activator.CreateInstance(i)!);
            _questTypes.Add(i);
        }
    }
    [NotMapped]
    public Tier QuestTier { get; private set; } = Tier.Bronze;
    ///<param name="messageToEdit"> if not null, should edit that message instead of adding a new message to the channel</param>
    /// <returns>True if quest was successfully completed</returns>
    public abstract Task<bool> StartQuest(InteractionContext context, DiscordMessage? messageToEdit = null);

    [NotMapped]
    public abstract IEnumerable<Reward> QuestRewards { get; protected set; }
    
    public ulong UserDataId { get; set; }
 
}