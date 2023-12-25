using System.Text;
using DiscordBotNet.Database.Models;
using DiscordBotNet.LegendaryBot.Entities;

namespace DiscordBotNet.LegendaryBot.Rewards;
/// <summary>
/// Rewards the user with entities to add to their inventoru
/// </summary>
public class EntityReward : Reward
{
    
    public IEnumerable<Entity> EntitiesToReward { get;  }
    public EntityReward(IEnumerable<Entity> entitiesToReward)
    {
        EntitiesToReward = entitiesToReward
            .Where(i => i is not null)
            .ToArray();
    }

    public override string GiveRewardTo(UserData userData, string name)
    {
        var stringBuilder = new StringBuilder($"Nice! {name} got: \n");
        userData.Inventory.AddRange(EntitiesToReward);


        Dictionary<string, int> nameSorter = [];
        foreach (var i in EntitiesToReward)
        {
            if(nameSorter.ContainsKey(i.Name))
            {
                nameSorter[i.Name] = 1;
          
            }
            else
            {
                nameSorter[i.Name]++;
            }
        }

        foreach (var i in nameSorter)
        {
            stringBuilder.Append($"{i.Key}: {i.Value}\n");
        }

        return stringBuilder.ToString();

    }
}