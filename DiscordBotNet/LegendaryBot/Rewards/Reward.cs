using DiscordBotNet.Database.Models;

namespace DiscordBotNet.LegendaryBot.Rewards;

public abstract class Reward
{
    public abstract string GiveRewardTo(UserData userData, string name);
}