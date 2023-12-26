using System.Text;
using DiscordBotNet.Database.Models;
using DiscordBotNet.LegendaryBot.Entities;

namespace DiscordBotNet.LegendaryBot.Rewards;
/// <summary>
/// Rewards the user with entities to add to their inventoru
/// </summary>
public class EntityReward : Reward
{
    public override int Priority => 2;

    public override Reward MergeWith(Reward reward)
    {
        if (reward is not EntityReward entityReward) throw new Exception("Reward type given is not of same type");
        return new EntityReward(EntitiesToReward.Union(entityReward.EntitiesToReward));
    }

    public override bool IsValid => EntitiesToReward.Count() > 0;
    public IEnumerable<Entity> EntitiesToReward { get;  }
    public EntityReward(IEnumerable<Entity> entitiesToReward)
    {
        EntitiesToReward = entitiesToReward
            .Where(i => i is not null)
            .ToArray();
    }

    public override string GiveRewardTo(UserData userData, string name)
    {
        var stringBuilder = new StringBuilder($"{name} got: \n");
        userData.Inventory.AddRange(EntitiesToReward);


        Dictionary<string, int> nameSorter = [];
        foreach (var i in EntitiesToReward)
        {
            var asString = i.ToString();
            if(nameSorter.ContainsKey(asString)) nameSorter[asString]++;
            else nameSorter[asString] = 1;
        }

        foreach (var i in nameSorter)
        {
            stringBuilder.Append($"{i.Key}: {i.Value}\n");
        }

        return stringBuilder.ToString();

    }
}